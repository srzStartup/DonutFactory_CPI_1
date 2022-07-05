using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Animations;

public class OvenSetupController : SetupController<OvenSettings>
{
    public override SetupControllerType type => SetupControllerType.Oven;

    [SerializeField] private Transform machineTransform;
    [SerializeField] private List<Transform> panShelves;
    [SerializeField] private Transform tweelBlock;
    [SerializeField] private CollectibleType relativeCollectibleType;
    [SerializeField] private TriggerIndicator triggerIndicator;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform timerRect;
    [SerializeField] private Image filledImage;
    [SerializeField] private RectTransform minuteHandParent;

    public int cookingTimeLevel;
    public int capacityLevel;
    public float cookingTime;
    public int capacity;
    public bool isCooking = false;
    public bool donutsReady = false;

    //Pan pan;
    Queue<Pan> panToCookQueue;
    Queue<Pan> readyPanQueue;
    Queue<Transform> panShelfQueue;

    float detachPanCooldown = .05f;
    float elapsedTime_DETACH;
    float takePanCooldown = .05f;
    float elapsedTime_TAKE_PAN;

    bool readyToCook = false;

    protected override void Start()
    {
        base.Start();

        panToCookQueue = new Queue<Pan>();
        readyPanQueue = new Queue<Pan>();
        panShelfQueue = new Queue<Transform>();

        cookingTime = settings.GetTimer(cookingTimeLevel);
        capacity = settings.GetCapacity(capacityLevel);

        for (int i = 0; i < capacity; i++)
        {
            panShelfQueue.Enqueue(panShelves[i]);
        }

        ConstraintSource constraintSource = new ConstraintSource
        {
            sourceTransform = CameraController.Instance.camTransform,
            weight = 1
        };
        _canvas.GetComponent<AimConstraint>().SetSource(0, constraintSource);
    }

    void LateUpdate()
    {
        if (panToCookQueue.Count != 0 /*&& readyToCook*/ && !isCooking && readyPanQueue.Count == 0)
        {
            StartCooking();
        }
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.TryGetComponent(out PlayerController _))
    //    {
    //        triggerIndicator.Open();
    //    }
    //}

    //void OnTriggerStay(Collider other)
    //{
    //    if (other.TryGetComponent(out PlayerController player) && player.state == PlayerState.Idle && !isCooking)
    //    {
    //        if (!donutsReady && player.taskState == PlayerTaskState.CarryingPan /*&& CanGetPanShelf()*/)
    //        {
    //            Utils.WithCooldownPassOneParam(detachPanCooldown, ref elapsedTime_DETACH, player, DetachItemFromPlayer);
    //        }
    //        //else if (donutsReady && player.stackManager.GetHoldCount() <= player.settings.GetCapacity(player.capacityLevel))
    //        //{
    //        //    Utils.WithCooldownPassOneParam(takePanCooldown, ref elapsedTime_TAKE_PAN, player, MakePlayerTakePan);
    //        //}
    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    if (other.TryGetComponent(out PlayerController _))
    //    {
    //        triggerIndicator.Close();
    //    }
    //}

    void DetachItemFromPlayer(PlayerController player)
    {
        Transform panShelf = GetPanShelf();
        Collectible panCollectible = player.DetachItem(CollectibleType.PanWithRawDonuts, panShelf.position, panShelf, onStart: OnDetachItemStart).Item1;

        if (panCollectible)
        {
            panCollectible.transform.parent = panShelf;
            panToCookQueue.Enqueue(panCollectible.GetComponent<Pan>());
        }
    }

    void OnDetachItemStart(Collectible _, Sequence tween)
    {
        tweelBlock.DOLocalRotate(new Vector3(90f, .0f, .0f), tween.Duration() / 2)
            .OnComplete(() => 
            {
                tweelBlock.DOLocalRotate(Vector3.zero, tween.Duration() / 2);
                //readyToCook = true;
            });
    }

    void MakePlayerTakePan(PlayerController player)
    {
        if (readyPanQueue.Count != 0)
        {
            Pan pan = readyPanQueue.Dequeue();
            Sequence panSequence = player.TakePan(pan, out bool success);

            if (success)
            {
                panSequence.Append(tweelBlock.DOLocalRotate(Vector3.zero, .5f)
                    .OnComplete(() =>
                    {
                        if (readyPanQueue.Count == 0)
                        {
                            donutsReady = false;
                        }
                    }));
            }

            if (player.taskState == PlayerTaskState.CarryingPan /*&& CanGetPanShelf()*/)
            {
                Transform panShelf = GetPanShelf();

                (Collectible, Sequence) detachPan = player.DetachItem(CollectibleType.PanWithRawDonuts, panShelf.position, panShelf);
                Collectible panCollectible = detachPan.Item1;
                Tween panTween = detachPan.Item2;

                if (panCollectible)
                {
                    panSequence.Join(panTween.OnComplete(() => StartCooking()));
                }
            }
        }
    }

    void StartCooking()
    {
        timerRect.gameObject.SetActive(true);

        DOVirtual.Float(0f, 360f, cookingTime, value =>
        {
            filledImage.material.SetFloat("_Arc1", value);
            float rotZ = minuteHandParent.localRotation.z;
            rotZ -= value;
            minuteHandParent.localRotation = Quaternion.Euler(minuteHandParent.localRotation.x, minuteHandParent.localRotation.y, rotZ);
        })
            .OnStart(() => isCooking = true)
            .OnComplete(() =>
            {
                foreach (Pan pan in panToCookQueue)
                {
                    foreach (Transform slot in pan.slots)
                    {
                        Transform donutTransform = slot.GetChild(0);
                        Collectible donut = donutTransform.GetComponent<Collectible>();

                        donutTransform.Find("DonutRaw").gameObject.SetActive(false);
                        donutTransform.Find("DonutBaked").gameObject.SetActive(true);

                        donut.type = CollectibleType.DonutBaked;

                        pan.AsCollectible().type = CollectibleType.PanWithBakedDonuts;
                    }
                }

                int panToCookQueueCount = panToCookQueue.Count;
                for (int i = 0; i < panToCookQueueCount; i++)
                {
                    readyPanQueue.Enqueue(panToCookQueue.Dequeue());
                }

                tweelBlock.DOLocalRotate(new Vector3(90f, .0f, .0f), 1f)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() =>
                    {
                        //readyToCook = false;
                        isCooking = false;
                        donutsReady = true;
                        timerRect.gameObject.SetActive(false);
                        filledImage.material.SetFloat("_Arc1", 0f);
                    });
            });
    }

    Transform GetPanShelf()
    {
        return panShelves.Find(somePanShelf => panShelfQueue.Contains(somePanShelf) && somePanShelf.childCount == 0);
    }

    //bool CanGetPanShelf()
    //{
    //    return panShelves.Find(somePanShelf => panShelfQueue.Contains(somePanShelf) && somePanShelf.childCount == 0) != null ||

    //}

    public void HandleUpgradeTrigger()
    {
        UIManager.Instance.OpenSetupControllerUpgradePanel(this);
    }

    public void UpgradeCapacity()
    {
        if (capacityLevel < settings.capacities.Count)
        {
            capacityLevel++;
            capacity++;

            Transform panShelf = panShelves.Find(panShelf => !panShelf.gameObject.activeSelf);
            panShelf.gameObject.SetActive(true);
            panShelfQueue.Enqueue(panShelf);

            JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).capacityLevel = capacityLevel;
            JSONDataManager.Instance.SaveData();
        }
    }

    public void UpgradeCookingTime()
    {
        if (cookingTimeLevel < settings.timers.Count)
        {
            cookingTimeLevel++;
            cookingTime = settings.GetTimer(cookingTimeLevel);

            JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).speedLevel = cookingTimeLevel;
            JSONDataManager.Instance.SaveData();
        }
    }

    protected override void OnUnlock()
    {
        
    }
}
