using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Animations;

public class CustomerController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform loadsParent;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI orderCountText;
    public int orderCount;
    public int totalAmount;

    private StackManager<Collectible> _stackManager;
    public StackManager<Collectible> stackManager => _stackManager;
    public int collectedCount { get; private set; }

    private readonly string _movementBlendParamName = "_movement";
    private float _movementBlend;

    private Transform _transform;
    private Vector3 nextStackPosition;

    Vector3 previousPosition;

    void Start()
    {
        _transform = transform;
        _stackManager = new StackManager<Collectible>();
        HandleAnim(0);

        ConstraintSource constraintSource = new ConstraintSource
        {
            sourceTransform = CameraController.Instance.camTransform,
            weight = 1
        };
        _canvas.GetComponent<AimConstraint>().SetSource(0, constraintSource);

        orderCountText.text = orderCount.ToString();
    }

    void Update()
    {
        Vector3 dir = (_transform.position - previousPosition).normalized;

        previousPosition = _transform.position;

        float facingDirection = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

        if (facingDirection != 0)
        {
            Quaternion rot = Quaternion.Euler(facingDirection * Vector3.up);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, rot, 10 * Time.deltaTime);
        }
    }

    public void Move(Vector3 worldPosition, System.Action onStart = null, System.Action onComplete = null)
    {
        float speed = 5f;
        _transform.DOMove(worldPosition, speed)
            .SetSpeedBased()
            .OnStart(() =>
            {
                HandleAnim(1);
                onStart?.Invoke();
            })
            .OnComplete(() =>
            {
                HandleAnim(0);
                onComplete?.Invoke();
            });
    }

    public void MoveOnPath(Vector3[] path, System.Action onComplete = null)
    {
        float speed = 5f;
        _transform.DOPath(path, speed, PathType.CatmullRom)
            .SetSpeedBased()
            .SetEase(Ease.Linear)
            .OnStart(() => HandleAnim(5))
            .OnComplete(() => onComplete?.Invoke());
    }

    public void Collect(Collectible collectible, System.Action onComplete = null)
    {
        Vector3 currentStackPosition = nextStackPosition;
        collectible.transform.parent = loadsParent;

        nextStackPosition.y += loadsParent.InverseTransformPoint(collectible.topPoint.position).y - loadsParent.InverseTransformPoint(collectible.transform.position).y;

        Tween localMoveTween = collectible.transform.DOLocalMove(currentStackPosition, .25f)
            .OnStart(() => collectedCount++)
            .OnComplete(() => 
            {
                _stackManager.Push(collectible);
                totalAmount += collectible.worth;
                onComplete?.Invoke();
            });
    }

    private void HandleAnim(float animValue)
    {
        _movementBlend = Mathf.Lerp(animValue, _movementBlend, .2f);

        _animator.SetBool("_isHandsFree", _stackManager.empty);

        _animator.SetFloat(_movementBlendParamName, _movementBlend);
    }
}
