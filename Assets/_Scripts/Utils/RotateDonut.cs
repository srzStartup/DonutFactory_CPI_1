using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RotateDonut : MonoBehaviour
{
    Transform _transform;
    void Start()
    {
        //transform.DORotate(new Vector3(360f, 0f, 0f), 2, RotateMode.WorldAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
        _transform = transform;
    }

    void Update()
    {
        _transform.Rotate(1f, 0f, 0f);
        //_transform.RotateAround(_transform.position, new Vector3(1, 0, 0), 1);
    }
}
