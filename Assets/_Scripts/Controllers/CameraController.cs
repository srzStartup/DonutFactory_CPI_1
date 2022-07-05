using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform _cameraParent;
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private Camera _camera;
    private Transform _cameraTransform;

    [SerializeField, Range(.0f, 100.0f)] private float distance;
    [SerializeField, Range(1.0f, 20.0f)] private float lerpTime;

    [SerializeField] private InGameEventChannel inGameEventChannel;

    [Header("CPI camera position refrences")]
    [SerializeField] private Transform donutPreparerCamRefPoint1;
    [SerializeField] private Transform donutPreparerCamRefPoint2;
    [SerializeField] private Transform ovenCamRefPoint;

    #region Camera Structure

    public Transform cameraParent => _cameraParent;
    public Transform cameraHolder => _cameraHolder;
    public Camera cam => _camera;
    public Transform camTransform => _cameraTransform;

    #endregion

    private bool _isFollowing = true;

    private Vector3 cameraHolderOffset;

    private Vector3 targetPosition;

    protected override void Awake()
    {
        base.Awake();

        inGameEventChannel.PasteConsumeByDonutRawPreparerEvent += OnPasteConsumeByDonutRawPreparer;
        inGameEventChannel.SendingRawDonutsToPanSequenceStartEvent += OnSendingRawDonutsToPanSequenceStart;
        inGameEventChannel.PanWithRawDonutsConsumeByOvenEvent += OnPanWithRawDonutsConsumeByOven;
    }

    void OnDestroy()
    {
        inGameEventChannel.PasteConsumeByDonutRawPreparerEvent -= OnPasteConsumeByDonutRawPreparer;
        inGameEventChannel.SendingRawDonutsToPanSequenceStartEvent -= OnSendingRawDonutsToPanSequenceStart;
        inGameEventChannel.PanWithRawDonutsConsumeByOvenEvent -= OnPanWithRawDonutsConsumeByOven;
    }

    void OnPasteConsumeByDonutRawPreparer()
    {
        FocusWithHolder(donutPreparerCamRefPoint1.position, donutPreparerCamRefPoint1.rotation.eulerAngles);
    }

    void OnSendingRawDonutsToPanSequenceStart()
    {
        Tween camHolderMoveTween = _cameraHolder.DOMove(donutPreparerCamRefPoint2.position, 3f)
            .SetSpeedBased()
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _cameraParent.position = target.position;
                _cameraHolder.DOLocalMove(cameraHolderOffset, 1f);
                _cameraHolder.DOLocalRotate(Vector3.zero, 2f);
                _cameraTransform.DOLocalMove(Vector3.zero, 1f)
                    .OnComplete(() =>
                    {
                        _isFollowing = true;
                        enabled = true;
                    });
            });

        _cameraTransform.DOLocalMove(new Vector3(0f, 4f, -5f), camHolderMoveTween.Duration());
    }

    void OnPanWithRawDonutsConsumeByOven()
    {
        FocusWithHolder(ovenCamRefPoint.position, ovenCamRefPoint.rotation.eulerAngles);
    }

    void Start()
    {
        BuildStruct();
        cameraHolderOffset = _cameraHolder.localPosition;
        targetPosition = _cameraParent.position;
    }

    void FixedUpdate()
    {
        if (_isFollowing)
        {
            targetPosition = target.position;
            targetPosition.y = _cameraParent.position.y;

            _cameraParent.position = Vector3.Lerp(_cameraParent.position, targetPosition, Time.deltaTime * lerpTime);
        }
    }

    public void LockAndFocus(Vector3 position, float delay = .0f, System.Action<Sequence> onStart = null, System.Action<Sequence> onComplete = null)
    {
        _isFollowing = false;

        Sequence sequence = DOTween.Sequence();

        Tween camParentMoveTween = _cameraParent.DOMove(position, 1f)
            .OnStart(() => enabled = false)
            .SetEase(Ease.InQuad)
            .OnComplete(() => _isFollowing = true);

        Vector3 camHolderPos = cameraHolderOffset;
        camHolderPos.y = _cameraHolder.localPosition.y;
        Tween camHolderMoveTween = _cameraHolder.DOLocalMove(cameraHolderOffset, 1f);

        sequence.Join(camParentMoveTween)
            .Join(camHolderMoveTween)
            .SetDelay(delay)
            .OnStart(() => onStart?.Invoke(sequence))
            .OnComplete(() => onComplete?.Invoke(sequence));
    }

    public void BackToTarget(float delay = .0f, System.Action<Sequence> onStart = null, System.Action<Sequence> onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();

        Tween camParentMoveTween = _cameraParent.DOMove(targetPosition, 1f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => 
            {
                _isFollowing = true;
                enabled = true;
            });

        sequence.Join(camParentMoveTween)
            .SetDelay(delay)
            .OnStart(() => onStart?.Invoke(sequence))
            .OnComplete(() => onComplete?.Invoke(sequence));
    }

    public void FocusWithHolder(Vector3 position, Vector3 rotation, float delay = .0f, System.Action<Sequence> onStart = null, System.Action<Sequence> onComplete = null)
    {
        float duration = 1f;
        Sequence sequence = DOTween.Sequence();

        Tween camHolderMoveTween = _cameraHolder.DOMove(position, duration)
            .SetEase(Ease.InQuad);

        Tween camRotationTween = _cameraHolder.DORotate(rotation - camTransform.rotation.eulerAngles, duration);

        sequence
            .Join(camHolderMoveTween)
            .Join(camRotationTween)
            .SetDelay(delay)
            .OnStart(() => 
            {
                _isFollowing = false;
                enabled = false;
                onStart?.Invoke(sequence);
            })
            .OnComplete(() => 
            {
                onComplete?.Invoke(sequence);
            });
    }

    void BuildStruct()
    {
        _camera = _camera != null ? _camera : GetComponent<Camera>() ? GetComponent<Camera>() : Camera.main;
        _cameraHolder = _cameraHolder == null ? _camera.transform.parent : _cameraHolder;
        _cameraParent = _cameraParent == null ? _cameraHolder.parent : _cameraParent;
        _cameraTransform = _camera.transform;
    }
}