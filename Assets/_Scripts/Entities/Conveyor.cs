using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Conveyor : MonoBehaviour
{
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private bool isXBased = false;

    Material _beltReference;

    public Transform startPoint;
    public Transform endPoint;

    Renderer meshRenderer;
    float offset;

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        _beltReference = meshRenderer.material;
    }

    void Update()
    {
        offset = Time.time * speed;

        Vector2 tilingOffset = Vector2.zero;

        if (isXBased)
            tilingOffset.x = offset;
        else
            tilingOffset.y = offset;

        _beltReference.mainTextureOffset = tilingOffset;
    }
}
