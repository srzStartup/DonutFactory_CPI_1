using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    [SerializeField] private Transform _core;
    [SerializeField] private Transform _topPoint;

    public Transform topPoint => _topPoint;
    public int worth;
}
