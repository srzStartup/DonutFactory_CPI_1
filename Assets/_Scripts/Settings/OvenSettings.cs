using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller Settings/OvenSettings")]
public class OvenSettings : ControllerSettings
{
    public List<OvenTimer> timers;
    public List<OvenCapacity> capacities;

    public float GetTimer(int level)
    {
        return timers.Find(timer => timer.level == level).cookingTime;
    }

    public int GetCapacity(int level)
    {
        return capacities.Find(capacity => capacity.level == level).capacity;
    }

    public int GetTimerUpgradePrice(int level)
    {
        return timers.Find(timer => timer.level == level).upgradePrice;
    }

    public int GetCapacityUpgradePrice(int level)
    {
        return capacities.Find(speed => speed.level == level).upgradePrice;
    }
}

[System.Serializable]
public class OvenTimer
{
    public int level;
    public float cookingTime;
    public int upgradePrice;
}

[System.Serializable]
public class OvenCapacity
{
    public int level;
    public int capacity;
    public int upgradePrice;
}
