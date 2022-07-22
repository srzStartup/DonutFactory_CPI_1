using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SauceSpillerSetupController : SetupController<SauceSpillerSettings>
{
    public override SetupControllerType type => SetupControllerType.SauceSpiller;

    [SerializeField] private Transform machineTransform;
    [SerializeField] private Transform panRefPoint;
    [SerializeField] private TriggerIndicator triggerIndicator;
    [SerializeField] private List<Transform> _slots;
    [SerializeField] private Conveyor conveyor;

    Queue<Transform> slotQueue;
    Queue<Collectible> donutQueue;
    Pan currentPan;
    Stack<Collectible> readyDonuts;

    [SerializeField] private string typeAsString;
    [SerializeField] private CollectibleType collectibleType;

    float period = .75f;
    float nextTime;
    float startTime;

    // onplayertriggerstay settings
    float collectCooldown = .05f;
    float elapsedTime_COLLECT;
    float detachCooldown = .5f;
    float elapsedTime_DETACH;

    protected override void Start()
    {
        base.Start();

        donutQueue = new Queue<Collectible>();
        slotQueue = new Queue<Transform>();
        readyDonuts = new Stack<Collectible>();

        _slots.ForEach(slot => slotQueue.Enqueue(slot));

        for (int i = 0; i < 30; i++)
        {
            Transform slot = slotQueue.Dequeue();

            GameObject donutGO = ObjectPooler.Instance.SpawnFromPool("donut", slot.position, Quaternion.identity);
            Collectible donut = donutGO.GetComponent<Collectible>();
            donut.type = collectibleType;

            donut.transform.parent = slot;
            Vector3 slotPos = Vector3.zero;

            if (slot.childCount == 1)
            {
                slotPos = Vector3.zero;
            }
            else
            {
                slotPos.y += (slot.InverseTransformPoint(donut.topPoint.position).y - slot.InverseTransformPoint(donut.transform.position).y) * (slot.childCount - 1);
            }

            donut.transform.localPosition = slotPos;

            donutGO.transform.Find("DonutRaw").gameObject.SetActive(false);
            donutGO.transform.Find("DonutBaked").gameObject.SetActive(true);
            donutGO.transform.Find(typeAsString).gameObject.SetActive(true);
            readyDonuts.Push(donut);

            slotQueue.Enqueue(slot);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Open();
        }
    }

    void LateUpdate()
    {
        Utils.WithPeriod(ref nextTime, ref startTime, period, SendDonuts);
    }

    void SendDonuts()
    {
        if (donutQueue.Count != 0)
        {
            Collectible donut = donutQueue.Dequeue();

            donut.transform.DOJump(conveyor.startPoint.position, 1, 1, .5f)
                .OnComplete(() =>
                {
                    donut.transform.DOMove(conveyor.endPoint.position, 1.5f)
                        .OnComplete(() =>
                        {
                            Transform nextSlot = slotQueue.Dequeue();
                            Vector3 slotPos = nextSlot.position;
                            donut.transform.parent = nextSlot;
                            slotPos.y += (nextSlot.InverseTransformPoint(new Vector3(0f, donut.topPoint.position.y, 0f)).y) * nextSlot.childCount;

                            donut.transform.DOMove(slotPos, .5f)
                                .SetEase(Ease.Linear)
                                .OnStart(() => slotQueue.Enqueue(nextSlot))
                                .OnComplete(() => 
                                {
                                    readyDonuts.Push(donut);
                                    GameManager.Instance.inGameEventChannel.RaiseSaucedDonutReadyEvent();
                                });
                        });
                });
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (player.state == PlayerState.Idle && player.taskState == PlayerTaskState.CarryingPan && !currentPan)
            {
                Utils.WithCooldownPassOneParam(detachCooldown, ref elapsedTime_DETACH, player, DetachItemFromPlayer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Close();
        }
    }

    void DetachItemFromPlayer(PlayerController player)
    {
        player.DetachItem(CollectibleType.PanWithBakedDonuts, panRefPoint.position, transform, panRefPoint.rotation.eulerAngles, onStart: OnDetachItemStart, onComplete: OnDetachItemComplete);
    }

    void OnDetachItemStart(Collectible panAsCollectible, Tween panTween)
    {
        GameManager.Instance.inGameEventChannel.RaisePanWithBakedDonutsConsumeBySauceSpillerEvent();
        StartCoroutine(RaiseEventDelayed());
        //panAsCollectible.transform.DORotate(panRefPoint.rotation.eulerAngles, panTween.Duration());
    }

    IEnumerator RaiseEventDelayed()
    {
        yield return Utils.GetWaitForSeconds(1f);
        GameManager.Instance.inGameEventChannel.RaiseSendingBakedDonutsToSauceSequenceStartEvent();
    }

    void OnDetachItemComplete(Collectible panAsCollectible, Tween panTween)
    {
        if (panAsCollectible)
        {
            Pan pan = panAsCollectible.GetComponent<Pan>();

            if (pan)
            {
                Vector3 initScale = pan.transform.localScale;
                pan.transform.DOScale(Vector3.zero, 2f)
                    .OnStart(() =>
                    {
                        currentPan = pan;
                        foreach (Transform slot in currentPan.slots)
                        {
                            Transform donutTransform = slot.GetChild(0);
                            if (donutTransform)
                            {
                                donutTransform.parent = transform;
                                Collectible donut = donutTransform.GetComponent<Collectible>();

                                if (donut)
                                {
                                    donut.type = CollectibleType.DonutSauced;
                                    donut.SetWorth(5);
                                    donutQueue.Enqueue(donut);
                                }
                            }
                        }
                    })
                    .OnComplete(() =>
                    {
                        ObjectPooler.Instance.PushToQueue(panAsCollectible.objectPoolTag, pan.gameObject);
                        pan.transform.localScale = initScale;
                        currentPan = null;
                    });
            }
        }
    }

    public void OnPlayerTriggerStay(PlayerController player)
    {
        if (player.state == PlayerState.Idle)
        {
            if (readyDonuts.Count != 0 && player.stackManager.Count < player.stackCapacity)
            {
                if (elapsedTime_COLLECT >= collectCooldown)
                {
                    Collectible donut = readyDonuts.Pop();
                    player.Collect(donut);
                    elapsedTime_COLLECT = 0;
                }
                else
                {
                    elapsedTime_COLLECT += Time.deltaTime;
                }
            }
        }
    }

    public void OnPlayerTriggerExit(PlayerController _)
    {
        elapsedTime_COLLECT = 0f;
    }

    protected override void OnUnlock()
    {
        machineTransform.DORewind();
        machineTransform.DOPunchScale(Vector3.one * .1f, .5f)
            .OnComplete(() =>
            {
                GetComponent<Collider>().enabled = true;
            });
    }
}
