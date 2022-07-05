using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations;
using DG.Tweening;

public class OvenSetupControllerNEW : SetupController<OvenSettings>
{
    public override SetupControllerType type => SetupControllerType.Oven;

    [SerializeField] private Transform machineTransform;
    [SerializeField] private List<Transform> panShelves;
    [SerializeField] private Transform tweelBlock;

    [SerializeField] private TriggerIndicator triggerIndicator;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform timerRect;
    [SerializeField] private Image filledImage;
    [SerializeField] private RectTransform minuteHandParent;
    [SerializeField] private ParticleSystem veypParticle;

    public int cookingTimeLevel;
    public int capacityLevel;
    public float cookingTime;
    public int capacity;

    float detachPanCooldown = .15f;
    float elapsedTime_DETACH;
    float takePanCooldown = .05f;
    float elapsedTime_TAKE_PAN;

    Queue<Pan> panToBakeQueue;
    Queue<Pan> panBakedQueue;
    bool baking = false;
    bool noPanDetaching = true;
    bool isTweelBlockOpen = false;
    bool switchingPans = false;

    protected override void Start()
    {
        base.Start();

        cookingTime = settings.GetTimer(cookingTimeLevel);
        capacity = settings.GetCapacity(capacityLevel);

        panToBakeQueue = new Queue<Pan>();
        panBakedQueue = new Queue<Pan>();

        HandlePanShelves();
        FaceTheCamera();
    }

    void LateUpdate()
    {
        if (!baking && panToBakeQueue.Count != 0 && panBakedQueue.Count == 0 && noPanDetaching)
        {
            Bake();
        }

        if (panBakedQueue.Count == 0 && isTweelBlockOpen)
        {
            tweelBlock.DOLocalRotate(Vector3.zero, .25f)
                    .OnStart(() => isTweelBlockOpen = false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Open();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player) && player.state == PlayerState.Idle)
        {
            if (!baking)
            {
                if (TryGetPanShelf(out Transform panShelf) && !switchingPans)
                {
                    Utils.WithCooldownPassTwoParam(detachPanCooldown, ref elapsedTime_DETACH, player, panShelf, DetachItemFromPlayer);
                }

                if (panBakedQueue.Count != 0)
                {
                    if (player.stackManager.items.Find(item => item.type == CollectibleType.PanWithRawDonuts))
                    {
                        Utils.WithCooldown(detachPanCooldown, ref elapsedTime_TAKE_PAN, () =>
                        {
                            Pan panToSwitch = panBakedQueue.Dequeue();
                            Transform panToSwitchParent = panToSwitch.transform.parent;
                            player.SwitchPans(panToSwitch, CollectibleType.PanWithRawDonuts, out Pan pan, panToSwitchParent.position,
                                onStart: (_pan) => 
                                {
                                    switchingPans = true;
                                    _pan.transform.parent = panToSwitchParent;
                                },
                                onComplete: (_pan) => 
                                {
                                    panToBakeQueue.Enqueue(_pan);
                                    switchingPans = false;
                                });
                        });
                    }
                    else
                    {
                        if (player.CanTake(panBakedQueue.Peek().AsCollectible().holds))
                        {
                            Utils.WithCooldownPassOneParam(detachPanCooldown, ref elapsedTime_TAKE_PAN, player, MakePlayerTakePan);
                        }
                    }
                }
            }
        }
        else
        {
            noPanDetaching = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Close();
            elapsedTime_DETACH = 0f;
            elapsedTime_TAKE_PAN = 0f;
            noPanDetaching = true;
        }
    }

    void DetachItemFromPlayer(PlayerController player, Transform panShelf)
    {
        Collectible panAsCollectible = player.DetachItem(CollectibleType.PanWithRawDonuts,
            panShelf.position, panShelf, onStart: OnDetachItemStart, onComplete: OnDetachItemComplete).Item1;

        noPanDetaching = panAsCollectible == null;

        if (panAsCollectible)
        {
            panAsCollectible.transform.parent = panShelf;
        }
    }

    void OnDetachItemStart(Collectible collectible, Sequence sequence)
    {
        tweelBlock.DOLocalRotate(new Vector3(90f, .0f, .0f), sequence.Duration() / 2)
            .OnComplete(() => 
            {
                //tweelBlock.DOLocalRotate(Vector3.zero, sequence.Duration() / 2);
                isTweelBlockOpen = true;
            });
    }

    void OnDetachItemComplete(Collectible collectible, Sequence sequence)
    {
        panToBakeQueue.Enqueue(collectible.GetComponent<Pan>());
        GameManager.Instance.inGameEventChannel.RaisePanWithRawDonutsConsumeByOvenEvent();
    }

    void MakePlayerTakePan(PlayerController player)
    {
        Pan pan = panBakedQueue.Dequeue();

        player.TakePan(pan, out bool success);
    }

    //void MakePlayerTakePan(PlayerController player)
    //{
    //    Pan pan = panBakedQueue.Dequeue();
    //    Transform panShelf = pan.transform.parent;

    //    void OnDetachItemStartWhenTakingPan(Collectible collectible, Sequence sequence)
    //    {
    //        player.TakePan(pan, out bool success);
    //    }

    //    Collectible panAsCollectible = player.DetachItem(CollectibleType.PanWithRawDonuts,
    //        panShelf.position, panShelf, onStart: OnDetachItemStartWhenTakingPan, onComplete: OnDetachItemComplete).Item1;

    //    noPanDetaching = panAsCollectible == null;

    //    if (!panAsCollectible)
    //    {
    //        player.TakePan(pan, out bool success);
    //    }
    //}

    void Bake()
    {
        baking = true;

        if (!veypParticle.isPlaying)
        {
            veypParticle.Play();
        }

        Tween timeTween = DOVirtual.Float(0f, 360f, cookingTime, value =>
        {
            filledImage.material.SetFloat("_Arc1", value);
            float rotZ = minuteHandParent.localRotation.z;
            rotZ -= value;
            minuteHandParent.localRotation = Quaternion.Euler(minuteHandParent.localRotation.x, minuteHandParent.localRotation.y, rotZ);
        });

        timeTween.OnStart(() =>
        {
            if (isTweelBlockOpen)
            {
                tweelBlock.DOLocalRotate(Vector3.zero, .25f)
                    .OnStart(() => isTweelBlockOpen = false);
            }

            timerRect.gameObject.SetActive(true);
        });
        timeTween.OnComplete(() =>
        {
            foreach (Pan pan in panToBakeQueue)
            {
                foreach (Transform slot in pan.slots)
                {
                    Transform donutTransform = slot.GetChild(0);
                    Collectible donut = donutTransform.GetComponent<Collectible>();

                    donutTransform.Find("DonutRaw").gameObject.SetActive(false);
                    donutTransform.Find("DonutBaked").gameObject.SetActive(true);

                    donut.type = CollectibleType.DonutBaked;
                }

                pan.AsCollectible().type = CollectibleType.PanWithBakedDonuts;
            }

            int panToBakeQueueCount = panToBakeQueue.Count;
            for (int i = 0; i < panToBakeQueueCount; i++)
            {
                panBakedQueue.Enqueue(panToBakeQueue.Dequeue());
            }

            GameManager.Instance.inGameEventChannel.RaisePanWithBakedDonutsReadyEvent();

            tweelBlock.DOLocalRotate(new Vector3(90f, .0f, .0f), 1f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    isTweelBlockOpen = true;
                    baking = false;
                    timerRect.gameObject.SetActive(false);
                    filledImage.material.SetFloat("_Arc1", 0f);
                });

            veypParticle.Stop();
        });
    }

    bool TryGetPanShelf(out Transform panShelf)
    {
        panShelf = panShelves.Find(somePanShelf => somePanShelf.gameObject.activeSelf && somePanShelf.childCount == 0);

        return panShelf != null;
    }

    void HandlePanShelves()
    {
        int j = 0;
        foreach (Transform panShelf in panShelves)
        {
            if (j < capacity)
            {
                panShelf.gameObject.SetActive(true);
            }
            else
            {
                panShelf.gameObject.SetActive(false);
            }

            j++;
        }
    }

    void FaceTheCamera()
    {
        ConstraintSource constraintSource = new ConstraintSource
        {
            sourceTransform = CameraController.Instance.camTransform,
            weight = 1
        };
        _canvas.GetComponent<AimConstraint>().SetSource(0, constraintSource);
    }

    #region Upgrade
    public void HandleUpgradeTrigger()
    {
        UIManager.Instance.OpenSetupControllerUpgradePanel(this);
    }

    public void UpgradeCapacity()
    {
        if (capacityLevel < settings.capacities.Count)
        {
            machineTransform.DORewind();
            machineTransform.DOPunchScale(Vector3.one * .1f, .5f)
                .OnComplete(() =>
                {
                    //capacityLevel++;
                    //capacity++;
                    //HandlePanShelves();

                    //JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).capacityLevel = capacityLevel;
                    //JSONDataManager.Instance.SaveData();
                });

            capacityLevel++;
            capacity++;
            HandlePanShelves();

            JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).capacityLevel = capacityLevel;
            JSONDataManager.Instance.SaveData();
        }
    }

    public void UpgradeCookingTime()
    {
        if (cookingTimeLevel < settings.timers.Count)
        {
            machineTransform.DORewind();
            machineTransform.DOPunchScale(Vector3.one * .1f, .5f)
                .OnComplete(() =>
                {
                    //cookingTimeLevel++;
                    //cookingTime = settings.GetTimer(cookingTimeLevel);

                    //JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).speedLevel = cookingTimeLevel;
                    //JSONDataManager.Instance.SaveData();
                });

            cookingTimeLevel++;
            cookingTime = settings.GetTimer(cookingTimeLevel);

            JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).speedLevel = cookingTimeLevel;
            JSONDataManager.Instance.SaveData();
        }
    }
    #endregion

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
