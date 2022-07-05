using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DonutRawPreparerController : SetupController<DonutRawPreparerSettings>
{
    public override SetupControllerType type => SetupControllerType.DonutRawPreparer;

    [SerializeField] private Transform machineTransform;
    [SerializeField] private Transform pasteBasket;
    [SerializeField] private Transform pasteContainer;
    [SerializeField] private Transform pasteEndpoint;
    [SerializeField] private Transform cylinderPan;
    [SerializeField] private Conveyor conveyor;

    [SerializeField] private Transform panRefPoint;
    [SerializeField] private Transform panStackParent;
    [SerializeField] private TriggerIndicator triggerIndicator;

    [SerializeField] private List<Slot> cylinderPanSlots;

    [SerializeField] private SkinnedMeshRenderer containerPasteBlend;
    [SerializeField] private SkinnedMeshRenderer pasteBasketPasteBlend;
    [SerializeField] private ParticleSystem doughFallParticle;

    public int capacityLevel;
    public int pasteCapacity;
    public int pasteProductivity;
    public int panCapacity;

    Queue<DonutPaste> pasteQueue;
    Queue<Collectible> donutTempQueue;
    Queue<Collectible> readyPanQueue;
    Queue<Slot> cylinderPanSlotQueue;
    int cylinderPanSlotQueueCount;

    DonutPaste currentPaste;
    Pan currentPan;

    bool preparingProcessActive = false;
    bool isPasteBasketFull = false;
    float cylinderPanTurningDuration = 1f;
    public int totalPaste => (currentPaste == null ? 0 : 1) + pasteQueue.Count + donutTempQueue.Count;
    public int totalPan => (currentPan == null ? 0 : 1) + readyPanQueue.Count;

    float pasteDetachCooldown = .05f;
    float elapsedTime_PASTE;
    float panDetachCooldown = .25f;
    float elapsedTime_PAN;
    float pasteParticleCooldown;
    float elapsedTime_PASTE_PARTICLE;

    bool canPlayerCollect = true;
    float targetContainerBlendShapeValue;
    float currentContainerBlendShapeValue;
    float blendShapeChangerValue = 20.0f;
    Tween containerBlendShapeValueChangerTween;

    bool canParticlePlay = false;
    int filledSlotCount;

    protected override void Start()
    {
        base.Start();

        cachedTransform = transform;

        pasteQueue = new Queue<DonutPaste>();
        donutTempQueue = new Queue<Collectible>();
        readyPanQueue = new Queue<Collectible>();
        cylinderPanSlotQueue = new Queue<Slot>();

        cylinderPanSlots.ForEach(slot => cylinderPanSlotQueue.Enqueue(slot));

        pasteCapacity = panCapacity = settings.GetCapacity(capacityLevel);

        if (isUnlocked)
        {
            GetComponent<Collider>().enabled = true;
            SpawnPanWithAnimation();
        }

        pasteParticleCooldown = doughFallParticle.main.duration;
        cylinderPanSlotQueueCount = cylinderPanSlotQueue.Count;
    }

    void FixedUpdate()
    {
        if (currentContainerBlendShapeValue != targetContainerBlendShapeValue)
        {
            currentContainerBlendShapeValue = targetContainerBlendShapeValue;
            if (containerBlendShapeValueChangerTween != null || containerBlendShapeValueChangerTween.IsActive())
            {
                containerBlendShapeValueChangerTween.Kill();
                containerBlendShapeValueChangerTween = null;
            }

            containerBlendShapeValueChangerTween = DOVirtual.Float(currentContainerBlendShapeValue, targetContainerBlendShapeValue, .2f, value =>
            {
                containerPasteBlend.SetBlendShapeWeight(0, value);
            }).OnComplete(() => containerBlendShapeValueChangerTween = null);
        }
    }

    void LateUpdate()
    {
        if (currentPaste != null)
        {
            if (!preparingProcessActive && isPasteBasketFull)
            {
                preparingProcessActive = true;
                StartPrepareDonutRawSequence();
            }
            if (preparingProcessActive && doughFallParticle.isStopped && canParticlePlay)
            {
                doughFallParticle.Play();
            }
        }
        else
        {
            if (!cylinderPanSlotQueue.Peek().isEmpty)
            {
                if (currentPan)
                {
                    SendDonutsToPan(onComplete: OnSendDonutsToPanSequenceComplete);
                }
            }

            if (pasteQueue.Count != 0)
            {
                PutPasteToBasket();
            }
        }

        if (!currentPan && readyPanQueue.Count < panCapacity)
        {
            SpawnPanWithAnimation();
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
            if (totalPaste < pasteCapacity)
            {
                Utils.WithCooldownPassOneParam(pasteDetachCooldown, ref elapsedTime_PASTE, player, DetachItemFromPlayer);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Close();
            elapsedTime_PASTE = 0f;
        }
    }

    void MakePlayerTakePan(PlayerController player)
    {
        player.TakePan(readyPanQueue.Dequeue().GetComponent<Pan>(), out bool success);
    }

    void DetachItemFromPlayer(PlayerController player)
    {
        player.DetachItem(CollectibleType.Paste, pasteEndpoint.position, pasteContainer,
            onStart: OnDetachItemSequenceStart, onComplete: OnDetachItemSequenceComplete, tweenCallback: DetachItemTweenCallback);
    }

    void OnDetachItemSequenceStart(Collectible collectible, Sequence sequence)
    {
        canPlayerCollect = false;
        donutTempQueue.Enqueue(collectible);
        GameManager.Instance.inGameEventChannel.RaisePasteConsumeByDonutRawPreparerEvent();
    }

    void OnDetachItemSequenceComplete(Collectible collectible, Sequence sequence)
    {
        Transform collectibleTransform = collectible.transform;

        Sequence pasteSequence = DOTween.Sequence();

        float tweenDuration = .75f;
        Vector3 initScale = collectibleTransform.localScale;
        Vector3 scaleChanged = initScale;
        scaleChanged.x *= .75f;
        float currentWeight = containerPasteBlend.GetBlendShapeWeight(0);
        float increase = currentWeight + 20f;

        Tween punchScaleTween = collectibleTransform.DOPunchScale(new Vector3(.05f, 0, 0), .25f);
        Tween scaleXDownTween = collectibleTransform.DOBlendableScaleBy(new Vector3(-.2f, 0, 0), tweenDuration);
        Tween moveYTween = collectibleTransform.DOMoveY(collectibleTransform.position.y - 1, tweenDuration);
        Tween scaleDownToZeroTween = collectibleTransform.DOBlendableScaleBy(-collectibleTransform.localScale, .25f)
            .OnStart(() =>
            {
                if (targetContainerBlendShapeValue + blendShapeChangerValue < 100f)
                {
                    targetContainerBlendShapeValue += blendShapeChangerValue;
                }
                else
                {
                    targetContainerBlendShapeValue = 100f;
                }
            })
            .OnComplete(() =>
            {
                collectible.ResetAll();
                ObjectPooler.Instance.PushToQueue("paste", collectible.gameObject);
            });

        pasteSequence.Insert(0f, punchScaleTween)
            .Insert(.25f, scaleXDownTween)
            .Insert(.25f, moveYTween)
            .Insert(tweenDuration, scaleDownToZeroTween)
            .OnComplete(() =>
            {
                donutTempQueue.Dequeue();
                pasteQueue.Enqueue(new DonutPaste(pasteProductivity));
                canPlayerCollect = true;
            });
    }


    void PutPasteToBasket()
    {
        float duration = 1.5f;

        Tween basketBlendShapeTween = DOVirtual.Float(0, 100, duration, value =>
        {
            pasteBasketPasteBlend.SetBlendShapeWeight(0, value);
        }).OnStart(() =>
        {
            currentPaste = pasteQueue.Dequeue();
            if (targetContainerBlendShapeValue - blendShapeChangerValue >= 0f)
            {
                targetContainerBlendShapeValue -= blendShapeChangerValue;
            }
            else
            {
                targetContainerBlendShapeValue = 0f;
            }
        }).OnComplete(() => isPasteBasketFull = true);

    }

    void SpawnPanWithAnimation()
    {
        currentPan = SpawnPan(false);

        Vector3 initPosition = currentPan.transform.position;
        Vector3 spawnPosition = currentPan.transform.position;
        spawnPosition.y += 1f;
        currentPan.transform.position = spawnPosition;

        Sequence sequence = DOTween.Sequence();
        Tween jumpTween = currentPan.transform.DOMove(initPosition, .25f)
            .SetEase(Ease.InBounce);

        Tween punchScaleTween = currentPan.transform.DOPunchScale(Vector3.one * .25f, .25f);

        sequence.Append(jumpTween).Append(punchScaleTween)
            .OnStart(() => currentPan.gameObject.SetActive(true))
            .OnComplete(() =>
            {
                if (!cylinderPanSlotQueue.Peek().isEmpty)
                {
                    SendDonutsToPan(onComplete: OnSendDonutsToPanSequenceComplete);
                }
            });
    }

    float GetCollectibleHeight(Collectible collectible)
    {
        return collectible.topPoint.position.y - collectible.transform.position.y;
    }

    Vector3 CalculatePanPosition()
    {
        Vector3 position = panRefPoint.position;

        return position;
    }

    void StartPrepareDonutRawSequence()
    {
        if (!cylinderPanSlotQueue.ToArray()[1].isEmpty)
        {
            canParticlePlay = false;
            GameManager.Instance.inGameEventChannel.RaiseSendingRawDonutsToPanSequenceStartEvent();
        }
        else
        {
            canParticlePlay = true;
        }

        if (!cylinderPanSlotQueue.Peek().isEmpty)
        {
            if (currentPan)
            {
                SendDonutsToPan(onComplete: OnSendDonutsToPanSequenceComplete);
            }
        }
        else
        {
            PrepareDonutRaw();
        }
    }

    void PrepareDonutRaw()
    {
        preparingProcessActive = true;
        Slot slot = cylinderPanSlotQueue.Dequeue();

        GameObject donutRawGO = ObjectPooler.Instance
            .SpawnFromPool("donut", transform.position, Quaternion.identity, false);

        if (donutRawGO)
        {
            float preparingDuration = 1f;
            Sequence sequence = DOTween.Sequence();

            float pasteBasketBlendShapeWeight = pasteBasketPasteBlend.GetBlendShapeWeight(0);
            Tween pasteBasketBlendTween = DOVirtual.Float(pasteBasketBlendShapeWeight, pasteBasketBlendShapeWeight - (100 / pasteProductivity), preparingDuration, value =>
            {
                pasteBasketPasteBlend.SetBlendShapeWeight(0, value);
            });

            sequence.Insert(0f, pasteBasketBlendTween);

            Transform donutRawTransform = donutRawGO.transform;
            donutRawTransform.parent = slot.transform;
            donutRawTransform.localPosition = Vector3.zero;
            donutRawTransform.localScale = Vector3.one;
            slot.retained = donutRawGO.GetComponent<Collectible>();

            Transform donutRawCoreTransform = donutRawTransform.Find("DonutRaw");
            SkinnedMeshRenderer donutRawRenderer = donutRawTransform.Find("DonutRaw").GetComponent<SkinnedMeshRenderer>();

            float timePosition = 0f;

            Vector3 donutRawCoreLocalPos = donutRawCoreTransform.localPosition;
            donutRawCoreLocalPos.y -= .675f;
            donutRawCoreTransform.localPosition = donutRawCoreLocalPos;
            donutRawGO.SetActive(true);

            int blendShapeCount = 4;

            Tween donutBlendUpTween3 = DOVirtual.Float(0, 100, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(3, value));
            timePosition = preparingDuration / blendShapeCount / 2 * ((float)3 / blendShapeCount);
            sequence.Insert(timePosition, donutBlendUpTween3);

            Tween donutBlendUpTween0 = DOVirtual.Float(0, 100, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(0, value));
            timePosition = preparingDuration / blendShapeCount / 2 * ((float)0 / blendShapeCount);
            sequence.Insert(timePosition, donutBlendUpTween0);

            Tween donutBlendUpTween1 = DOVirtual.Float(0, 100, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(1, value));
            timePosition = preparingDuration / blendShapeCount / 2 * ((float)1 / blendShapeCount);
            sequence.Insert(timePosition, donutBlendUpTween1);

            Tween donutBlendUpTween2 = DOVirtual.Float(0, 100, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(2, value));
            timePosition = preparingDuration / blendShapeCount / 2 * ((float)2 / blendShapeCount);
            sequence.Insert(timePosition, donutBlendUpTween2);

            Tween donutBlendDownTween3 = DOVirtual.Float(100, 0, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(3, value));
            timePosition = preparingDuration - (preparingDuration / blendShapeCount / 2 * ((float)3 / blendShapeCount));
            sequence.Insert(timePosition, donutBlendDownTween3);

            Tween donutBlendDownTween0 = DOVirtual.Float(100, 0, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(0, value));
            timePosition = preparingDuration - (preparingDuration / blendShapeCount / 2 * ((float)0 / blendShapeCount));
            sequence.Insert(timePosition, donutBlendDownTween0);

            Tween donutBlendDownTween1 = DOVirtual.Float(100, 0, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(1, value));
            timePosition = preparingDuration - (preparingDuration / blendShapeCount / 2 * ((float)1 / blendShapeCount));
            sequence.Insert(timePosition, donutBlendDownTween1);

            Tween donutBlendDownTween2 = DOVirtual.Float(100, 0, preparingDuration / blendShapeCount / 2, value => donutRawRenderer.SetBlendShapeWeight(2, value));
            timePosition = preparingDuration - (preparingDuration / blendShapeCount / 2 * ((float)2 / blendShapeCount));
            sequence.Insert(timePosition, donutBlendDownTween2);

            sequence.Insert(0f, donutRawCoreTransform.DOLocalMoveY(0f, preparingDuration * 2));

            sequence.OnComplete(() =>
            {
                filledSlotCount++;
                cylinderPanSlotQueue.Enqueue(slot);

                if (!cylinderPanSlotQueue.Peek().isEmpty)
                {
                    isPasteBasketFull = false;
                }

                cylinderPan.DORotate(new Vector3(.0f, 360f / cylinderPanSlots.Count * -1, .0f), cylinderPanTurningDuration, RotateMode.WorldAxisAdd)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        HandlePaste();
                        preparingProcessActive = false;
                    });
            });
        }
    }

    void SendDonutsToPan(System.Action onStart = null, System.Action onComplete = null)
    {
        filledSlotCount = 0;

        float animationLifetime = 2f;
        float timePosition      = 0f;
        int   index             = 0;
        int   realIndex         = 0;

        Sequence sequence = DOTween.Sequence();

        foreach (Slot filledSlot in cylinderPanSlotQueue)
        {
            Transform panSlot = currentPan.GetSlot(index);
            Collectible collectible = filledSlot.Release();

            Tween jumpTween = collectible.transform.DOJump(conveyor.startPoint.position, 1, 1, animationLifetime / 3f)
                .SetEase(Ease.Linear)
                .OnStart(() => collectible.transform.parent = panSlot);

            Tween moveOnConveyorTween = collectible.transform.DOMove(conveyor.endPoint.position, animationLifetime / 2f)
                .SetEase(Ease.Linear);
            Tween moveToPanTween = collectible.transform.DOLocalJump(Vector3.zero, 1f, 1, animationLifetime / 3f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (realIndex == cylinderPanSlotQueueCount - 1)
                    {
                        onComplete?.Invoke();
                    }

                    realIndex++;
                });

            timePosition = animationLifetime * ((float)index++ / cylinderPanSlotQueue.Count);

            sequence.Insert(timePosition, jumpTween)
                    .Insert(timePosition + (animationLifetime / 3f), moveOnConveyorTween)
                    .Insert(timePosition + (animationLifetime / 3f * 2), moveToPanTween);
        }

        sequence.OnStart(() => onStart?.Invoke());
    }

    void OnSendDonutsToPanSequenceComplete()
    {
        Collectible panAsCollectible = currentPan.GetComponent<Collectible>();
        panAsCollectible.type = CollectibleType.PanWithRawDonuts;

        MakeCurrentPanReady(
            onStart: () =>
            {
                readyPanQueue.Enqueue(panAsCollectible);
                currentPan = null;
            },
            onComplete: () =>
            {
                GameManager.Instance.inGameEventChannel.RaisePanWithRawDonutsReadyEvent();
            });
    }

    void MakeCurrentPanReady(System.Action onStart = null, System.Action onComplete = null)
    {
        Collectible panAsCollectible = currentPan.GetComponent<Collectible>();

        panAsCollectible.transform.parent = panStackParent;

        float animationLifetime = .25f;
        Sequence sequence = DOTween.Sequence();

        Tween moveTween = panAsCollectible.transform.DOLocalMove(Vector3.zero, animationLifetime);
        sequence.Join(moveTween);

        foreach (Collectible otherPanAsCollectible in readyPanQueue)
        {
            Vector3 pos = otherPanAsCollectible.transform.position;
            pos.y += GetCollectibleHeight(panAsCollectible);

            Vector3 localPos = otherPanAsCollectible.transform.parent.InverseTransformPoint(pos);
            localPos.x += 1f;

            Tween otherMoveTween = otherPanAsCollectible.transform.DOLocalMove(localPos, animationLifetime);

            sequence.Join(otherMoveTween);
        }

        sequence.OnStart(() => onStart?.Invoke())
                .OnComplete(() => onComplete?.Invoke());
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
                    //panCapacity++;
                    //pasteCapacity++;

                    //JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).capacityLevel = capacityLevel;
                    //JSONDataManager.Instance.SaveData();
                });

            capacityLevel++;
            panCapacity++;
            pasteCapacity++;

            JSONDataManager.Instance.data.setups.Find(setupData => setupData.id == id).capacityLevel = capacityLevel;
            JSONDataManager.Instance.SaveData();
        }
    }

    Tween DetachItemTweenCallback(Collectible collectible, float animationLifetime)
    {
        return collectible.transform.DOJump(pasteEndpoint.position, 2, 1, animationLifetime);
    }

    Pan SpawnPan(bool isActive = true)
    {
        GameObject panGO = ObjectPooler.Instance.SpawnFromPool("pan", CalculatePanPosition(), panRefPoint.rotation, isActive);

        return panGO.GetComponent<Pan>();
    }

    void HandlePaste()
    {
        currentPaste.Proceed();

        if (currentPaste.productivity == 0)
        {
            currentPaste = null;
        }
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

    public void HandleUpgradeTrigger()
    {
        UIManager.Instance.OpenSetupControllerUpgradePanel(this);
    }

    public void OnPlayerTriggerStay(PlayerController player)
    {
        if (canPlayerCollect && readyPanQueue.Count != 0 && player.CanTake(readyPanQueue.Peek().holds))
        {
            Utils.WithCooldownPassOneParam(panDetachCooldown, ref elapsedTime_PAN, player, MakePlayerTakePan);
        }
    }

    public void OnPlayerTriggerExit(PlayerController player)
    {
        elapsedTime_PAN = 0f;
    }
}

public class DonutPaste
{
    public int productivity { get; private set; }

    public DonutPaste(int productivity)
    {
        this.productivity = productivity;
    }

    public void Proceed()
    {
        productivity--;
    }
}
