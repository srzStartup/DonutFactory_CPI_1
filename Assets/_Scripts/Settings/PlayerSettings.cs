using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller Settings/Player Settings")]
public class PlayerSettings : ScriptableObject
{
    public List<PlayerSpeed> speeds;
    public List<PlayerStackCapacity> capacities;

    public float GetSpeed(int level)
    {
        return speeds.Find(speed => speed.level == level).speed;
    }

    public int GetCapacity(int level)
    {
        return capacities.Find(capacity => capacity.level == level).capacity;
    }

    public int GetSpeedUpgradePrice(int level)
    {
        return speeds.Find(speed => speed.level == level).upgradePrice;
    }

    public int GetCapacityUpgradePrice(int level)
    {
        return capacities.Find(speed => speed.level == level).upgradePrice;
    }
}

[System.Serializable]
public class PlayerSpeed
{
    public int level;
    public float speed;
    public int upgradePrice;
}
[System.Serializable]
public class PlayerStackCapacity
{
    public int level;
    public int capacity;
    public int upgradePrice;
}
