using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PlayerController : Singleton<PlayerController>, ICollector
{
    [SerializeField] private FloatingJoystick _floatingJoystick;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI maxText;
    [Range(0, 13)]
    [SerializeField] private float _angularSpeed;
    public float forwardSpeed;
    public int speedLevel;
    public PlayerSettings settings;

    [SerializeField] private Transform loadsParent;
    public int capacityLevel;
    public int stackCapacity;

    //private List<Collectible> collectedItems;
    public StackManager<Collectible> _stackManager;
    public Vector3 nextStackPosition;

    private readonly string _movementBlendParamName = "_movement";
    //private float _range;

    private Animator _animator;
    private Rigidbody _rigidbody;
    private Transform _transform;
    //private Vector3 _polePosition;

    private bool _isMoving;
    private float _movementBlend;

    public bool IsMoving => _isMoving;
    public float ForwardSpeed => forwardSpeed;
    public PlayerState state { get; private set; }
    public PlayerTaskState taskState;
    public StackManager<Collectible> stackManager => _stackManager;

    public bool canMove = true;

    void Start()
    {
        DOTween.useSafeMode = false;
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _stackManager = new StackManager<Collectible>();
        nextStackPosition = Vector3.zero;

        forwardSpeed = settings.GetSpeed(speedLevel);
        stackCapacity = settings.GetCapacity(capacityLevel);
    }
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 input = _floatingJoystick.Direction;

        if (!canMove)
        {
            input = Vector2.zero;
        }

        float facingDirection = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;

        if (facingDirection != 0)
        {
            Quaternion rot = Quaternion.Euler(facingDirection * Vector3.up);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, rot, _angularSpeed * Time.deltaTime);
        }

        _rigidbody.velocity = new Vector3(input.x, 0, input.y) * forwardSpeed;
        float animValue = input.magnitude;

        ChangeState(animValue == 0 ? PlayerState.Idle : PlayerState.Moving);

        HandleAnim(animValue);
    }

    private void HandleAnim(float animValue)
    {
        _movementBlend = Mathf.Lerp(animValue, _movementBlend, .2f);

        _animator.SetBool("_isHandsFree", _stackManager.empty);

        _animator.SetFloat(_movementBlendParamName, _movementBlend);
    }

    public void ChangeState(PlayerState state)
    {
        if (this.state == state)
            return;

        this.state = state;
        GameManager.Instance.inGameEventChannel.RaisePlayerStateChangedEvent(this.state);
    }

    public void ChangeTaskState(PlayerTaskState taskState)
    {
        if (this.taskState == taskState)
            return;

        this.taskState = taskState;
        //GameManager.Instance.inGameEventChannel.RaisePlayerTaskStateChangedEvent(this.taskState);
    }

    int pairing;
    public void Collect(Collectible collectible)
    {
        if (_stackManager.GetHoldCount() >= stackCapacity)
            return;

        Vector3 currentStackPosition = nextStackPosition;
        collectible.transform.parent = loadsParent;

        if (Collectible.IsDonut(collectible))
        {
            if (pairing++ == 0)
            {
                currentStackPosition.x -= .8f;
            }
            else
            {
                currentStackPosition.x += .8f;
                pairing = 0;

                nextStackPosition.y += loadsParent.InverseTransformPoint(collectible.topPoint.position).y - loadsParent.InverseTransformPoint(collectible.transform.position).y;
            }
        }
        else
        {
            nextStackPosition.y += loadsParent.InverseTransformPoint(collectible.topPoint.position).y - loadsParent.InverseTransformPoint(collectible.transform.position).y;
        }

        Sequence sequence = DOTween.Sequence();

        Tween localMoveTween = collectible.transform.DOLocalJump(currentStackPosition, 2f, 1, .25f)
        //Tween localMoveTween = collectible.transform.DOLocalMove(currentStackPosition, .25f)
            .OnStart(() =>
            {
                _stackManager.Push(collectible);
                Taptic.Light();
            })
            .OnComplete(() =>
            {
                if (_stackManager.GetHoldCount() == stackCapacity)
                {
                    ThrowMaxText();
                }
            });

        //Tween localRotationTween = collectible.transform.DOLocalRotate(Vector3.zero, .25f);

        if (Collectible.IsDonut(collectible))
        {
            Vector3 spin = new Vector3(-360f, 0f, 0f);
            Tween donutLocalRotationTween = collectible.transform.DOLocalRotate(spin, .25f, RotateMode.LocalAxisAdd);
            sequence.Join(donutLocalRotationTween);
            //sequence.Insert(0.1f, localRotationTween);
        }
        else
        {
            Tween localRotationTween = collectible.transform.DOLocalRotate(Vector3.zero, .25f);
            sequence.Join(localRotationTween);
        }

        sequence.Join(localMoveTween)/*.Join(localRotationTween)*/;
    }

    public (Collectible, Sequence) DetachItem(CollectibleType collectibleType, Vector3 worldPosition, Transform toParent = null, Vector3? worldRotation = null, bool noRotation = false,
        System.Action<Collectible, Sequence> onStart = null, System.Action<Collectible, Sequence> onComplete = null, System.Func<Collectible, float, Tween> tweenCallback = null)
    {
        if (_stackManager.Count == 0)
            return (null, null);

        Collectible reqCollectible;

        if (collectibleType == CollectibleType.Any)
        {
            reqCollectible = _stackManager.Last();
        }
        else if (collectibleType == CollectibleType.DonutSauced)
        {
            reqCollectible = _stackManager.items
                .FindLast(collectible => collectible.type == CollectibleType.DonutSaucedCaramel ||
                    collectible.type == CollectibleType.DonutSaucedChocolate ||
                    collectible.type == CollectibleType.DonutSaucedStrawberry ||
                    collectible.type == CollectibleType.DonutWithBonbon ||
                    collectible.type == CollectibleType.DonutWithSprinkles ||
                    collectible.type == CollectibleType.DonutWithOreo);
        }
        else if (collectibleType == CollectibleType.DonutSaucedWithoutCandy)
        {
            reqCollectible = _stackManager.items
                .FindLast(collectible => collectible.type == CollectibleType.DonutSaucedCaramel ||
                    collectible.type == CollectibleType.DonutSaucedChocolate ||
                    collectible.type == CollectibleType.DonutSaucedStrawberry);
        }
        else
        {
            reqCollectible = _stackManager.items.FindLast(collectible => collectible.type == collectibleType);
        }

        Sequence sequence = null;

        if (reqCollectible)
        {
            nextStackPosition.y -= loadsParent.InverseTransformPoint(reqCollectible.topPoint.position).y - loadsParent.InverseTransformPoint(reqCollectible.transform.position).y;

            sequence = DOTween.Sequence();
            int index = _stackManager.IndexOf(reqCollectible);
            float animationLifeTime = .5f;
            Vector3 absWorldRotation = (Vector3)(worldRotation == null ? Vector3.zero : worldRotation);

            Tween collectibleMoveTween;

            if (tweenCallback != null)
            {
                collectibleMoveTween = tweenCallback.Invoke(reqCollectible, animationLifeTime);
            }
            else
            {
                collectibleMoveTween = reqCollectible.transform.DOMove(worldPosition, animationLifeTime);
            }

            collectibleMoveTween.OnStart(() =>
            {
                if (noRotation)
                {
                    reqCollectible.ResetAllNoRotation();
                }
                else
                {
                    reqCollectible.ResetAll();
                }

                List<Collectible> rest = _stackManager.FromRange(index);

                //if (collectibleType == CollectibleType.PanWithRawDonuts || collectibleType == CollectibleType.PanWithBakedDonuts || collectibleType == CollectibleType.Pan)
                //{
                //    rest = _stackManager.FromRange(0);
                //}

                float diff = loadsParent.InverseTransformPoint(reqCollectible.topPoint.position).y - loadsParent.InverseTransformPoint(reqCollectible.transform.position).y;

                Sequence reOrderSequence = DOTween.Sequence();
                foreach (Collectible collectible in rest)
                {
                    reOrderSequence.Join(collectible.transform.DOLocalMoveY(loadsParent.InverseTransformPoint(collectible.transform.position).y - diff, animationLifeTime));
                }
            })
            .OnComplete(() => reqCollectible.transform.parent = toParent);

            Vector3 rot = reqCollectible.transform.rotation.eulerAngles - absWorldRotation;

            if (Collectible.IsDonut(reqCollectible))
            {
                Vector3 spin = new Vector3(720f, 0f, 0f);
                //Tween donutLocalRotationTween = reqCollectible.transform.DORotate(spin, animationLifeTime, RotateMode.WorldAxisAdd);
                //sequence.Join(donutLocalRotationTween);
                rot += spin;
            }
            else
            {
                rot = absWorldRotation;
            }


            Tween collectibleRotationTween = reqCollectible.transform.DORotate(rot, animationLifeTime, RotateMode.WorldAxisAdd);

            sequence.Join(collectibleMoveTween).Join(collectibleRotationTween);

            sequence.OnStart(() =>
            {
                _stackManager.Remove(reqCollectible);
                if (reqCollectible.type == CollectibleType.Pan || reqCollectible.type == CollectibleType.PanWithRawDonuts || reqCollectible.type == CollectibleType.PanWithBakedDonuts)
                {
                    System.Predicate<Collectible> match = item => item.type == CollectibleType.Pan || item.type == CollectibleType.PanWithRawDonuts || item.type == CollectibleType.PanWithBakedDonuts;
                    if (!_stackManager.items.Exists(match))
                    {
                        ChangeTaskState(PlayerTaskState.Available);
                    }
                }

                onStart?.Invoke(reqCollectible, sequence);
            })
                .OnComplete(() =>
                {
                    if (stackManager.empty)
                        nextStackPosition = Vector3.zero;
                    else
                    {
                        //nextStackPosition.y = stackManager.Last().topPoint.localPosition.y - 1f;
                    }

                    onComplete?.Invoke(reqCollectible, sequence);
                });
        }

        return (reqCollectible, sequence);
    }

    public Sequence TakePan(Pan pan, out bool success,
        float tweenDuration = .25f,
        System.Action<PlayerController> onStart = null,
        System.Action<PlayerController> onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();

        if (_stackManager.GetHoldCount() < stackCapacity && !_stackManager.items.Contains(pan.AsCollectible()))
        {
            stackManager.items.Insert(0, pan.AsCollectible());
            Taptic.Light();
            pan.transform.parent = loadsParent;

            float collectibleHeight = loadsParent.InverseTransformPoint(pan.AsCollectible().topPoint.position).y - loadsParent.InverseTransformPoint(pan.AsCollectible().transform.position).y;
            nextStackPosition.y += collectibleHeight;

            foreach (Collectible collectible in _stackManager.items.FindAll(item => item != pan.AsCollectible()))
            {
                Tween collectibleMoveYTween = collectible.transform.DOLocalMoveY(collectible.transform.localPosition.y + collectibleHeight, tweenDuration);
                sequence.Join(collectibleMoveYTween);
            }

            this.ChangeTaskState(PlayerTaskState.CarryingPan);

            Tween moveTween = pan.transform.DOLocalMove(Vector3.zero, tweenDuration);
            Tween rotationTween = pan.transform.DOLocalRotate(Vector3.zero, tweenDuration);

            sequence.Join(moveTween).Join(rotationTween)
                .OnStart(() =>
                {
                    if (_stackManager.GetHoldCount() == stackCapacity)
                    {
                        ThrowMaxText();
                    }

                    onStart?.Invoke(this);
                })
                .OnComplete(() => onComplete?.Invoke(this));

            success = true;
        }
        else success = false;

        return sequence;
    }

    public void SwitchPans(Pan panToSwitch, CollectibleType panCollectibleType,
        out Pan pan,
        Vector3 worldPosition, Transform toParent = null, Vector3? worldRotation = null,
        float tweenDuration = .25f,
        System.Action<Pan> onStart = null,
        System.Action<Pan> onComplete = null)
    {
        pan = null;
        Sequence sequence = DOTween.Sequence();

        Collectible panAsCollectible = _stackManager.items.Find(item => item.type == panCollectibleType);

        if (panAsCollectible && panAsCollectible.GetComponent<Pan>())
        {
            Vector3 panLocalPosition = loadsParent.InverseTransformPoint(panAsCollectible.transform.position);
            Vector3 absWorldRotation = (Vector3)(worldRotation == null ? Vector3.zero : worldRotation);

            int index = _stackManager.IndexOf(panAsCollectible);
            _stackManager.Remove(panAsCollectible);
            _stackManager.items.Insert(index, panToSwitch.AsCollectible());

            Taptic.Light();

            pan = panAsCollectible.GetComponent<Pan>();

            panToSwitch.transform.parent = loadsParent;
            pan.transform.parent = toParent;

            Tween detachedMoveTween = pan.transform.DOMove(worldPosition, tweenDuration);
            Tween detachedRotationTween = pan.transform.DORotate(absWorldRotation, tweenDuration);
            Tween moveTween = panToSwitch.transform.DOLocalMove(panLocalPosition, tweenDuration);
            Tween rotationTween = panToSwitch.transform.DOLocalRotate(Vector3.zero, tweenDuration);

            sequence.Join(moveTween).Join(rotationTween).Join(detachedMoveTween).Join(detachedRotationTween)
                .OnStart(() => onStart?.Invoke(panAsCollectible.GetComponent<Pan>()))
                .OnComplete(() => onComplete?.Invoke(panAsCollectible.GetComponent<Pan>()));
        }
    }

    public void CollectMoney(Money money)
    {
        money.transform.DOMove(loadsParent.position, .1f)
            .SetEase(Ease.Linear)
            .OnStart(() => Taptic.Light())
            .OnComplete(() =>
            {
                GameManager.Instance.UpdateMoney(money.worth);
                ObjectPooler.Instance.PushToQueue("money", money.gameObject);
            });
    }

    public bool CanTake(int holds)
    {
        return _stackManager.GetHoldCount() + holds <= stackCapacity;
    }

    public bool CanTake(Collectible collectible)
    {
        return _stackManager.GetHoldCount() + collectible.holds <= stackCapacity;
    }

    void ThrowMaxText()
    {
        Vector3 textPosition = maxText.transform.position;
        Vector3 lastItemPosition = _stackManager.Last().transform.position;
        textPosition.y = lastItemPosition.y + 5f;
        maxText.transform.position = textPosition;
        StartCoroutine(WaitAndDisappear());
    }

    IEnumerator WaitAndDisappear()
    {
        yield return Utils.GetWaitForSeconds(.1f);
        maxText.gameObject.SetActive(true);
        yield return Utils.GetWaitForSeconds(1.5f);
        maxText.gameObject.SetActive(false);
    }

    public void UpgradeSpeed()
    {
        if (speedLevel < settings.speeds.Count)
        {
            speedLevel++;
            forwardSpeed = settings.GetSpeed(speedLevel);

            JSONDataManager.Instance.data.playerSpeedLevel = speedLevel;
            JSONDataManager.Instance.SaveData();

            _transform.DORewind();
            _transform.DOPunchScale(Vector3.one * .25f, .2f);
        }
    }

    public void UpgradeCapacity()
    {
        if (capacityLevel < settings.capacities.Count)
        {
            capacityLevel++;
            stackCapacity = settings.GetCapacity(capacityLevel);

            JSONDataManager.Instance.data.playerCapacityLevel = capacityLevel;
            JSONDataManager.Instance.SaveData();

            _transform.DORewind();
            _transform.DOPunchScale(Vector3.one * .1f, .2f);
        }
    }
}

public enum PlayerState
{
    Idle,
    Moving,
}

public enum PlayerTaskState
{
    Available,
    CarryingPan,
}
