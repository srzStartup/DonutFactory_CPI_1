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

    public static bool IsDonut(Collectible collectible)
    {
        return
            collectible.type == CollectibleType.DonutBaked ||
            collectible.type == CollectibleType.DonutRaw ||
            collectible.type == CollectibleType.DonutSauced ||
            collectible.type == CollectibleType.DonutSaucedCaramel ||
            collectible.type == CollectibleType.DonutSaucedChocolate ||
            collectible.type == CollectibleType.DonutSaucedStrawberry ||
            collectible.type == CollectibleType.DonutSaucedWithoutCandy ||
            collectible.type == CollectibleType.DonutWithBonbon ||
            collectible.type == CollectibleType.DonutWithOreo ||
            collectible.type == CollectibleType.DonutWithSprinkles;

    }

    public void ResetAll()
    {
        isTargeted = false;
        isCollected = false;

        transform.rotation = Quaternion.identity;
    }

    public void ResetAllNoRotation()
    {
        isTargeted = false;
        isCollected = false;
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
