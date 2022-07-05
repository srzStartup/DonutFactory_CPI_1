using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour, IStackable
{
    public string objectPoolTag;
    public int worth;
    public bool isTargeted = false;
    public bool isCollected = false;
    public Transform topPoint;
    public Transform core;
    public CollectibleType type;
    [SerializeField] private int _holds;
    public int holds => _holds;

    public void ResetAll()
    {
        isTargeted = false;
        isCollected = false;

        transform.rotation = Quaternion.identity;
    }

    public void SetWorth(int worth)
    {
        this.worth = worth;
    }
}

public enum CollectibleType
{
    Paste,
    DonutRaw,
    DonutBaked,
    DonutSauced,
    DonutSaucedWithoutCandy,
    DonutSaucedChocolate,
    DonutSaucedStrawberry,
    DonutSaucedCaramel,
    DonutWithSprinkles,
    DonutWithBonbon,
    DonutWithOreo,
    Pan,
    PanWithRawDonuts,
    PanWithBakedDonuts,
    Any,
}
