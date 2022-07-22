using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public interface ICollector
{
    public void Collect(Collectible collectible);
    public (Collectible, Sequence) DetachItem(CollectibleType collectibleType, Vector3 worldPosition, Transform toParent = null, Vector3? worldRotation = null, bool noRotation = false,
        System.Action<Collectible, Sequence> onStart = null, System.Action<Collectible, Sequence> onComplete = null, System.Func<Collectible, float, Tween> tweenCallback = null);
}

public class DetachItemInfo
{
    public CollectibleType collectibleType;
    public Vector3 worldPosition;
    public Transform toParent;
    public Vector3 worldRotation;
    System.Action<Collectible, Sequence> onStart;
    System.Action<Collectible, Sequence> onComplete;

    public DetachItemInfo(CollectibleType collectibleType, Vector3 worldPosition)
    {
        this.collectibleType = collectibleType;
        this.worldPosition = worldPosition;
    }

    public DetachItemInfo SetWorldRotation(Vector3 worldRotation)
    {
        this.worldRotation = worldRotation;
        return this;
    }

    public DetachItemInfo SetParent(Transform toParent)
    {
        this.toParent = toParent;
        return this;
    }

    public DetachItemInfo OnStart(System.Action<Collectible, Sequence> onStart)
    {
        this.onStart = onStart;
        return this;
    }

    public DetachItemInfo OnComplete(System.Action<Collectible, Sequence> onComplete)
    {
        this.onComplete = onComplete;
        return this;
    }
}
