using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller Settings/DonutRawPreparerSettings")]
public class DonutRawPreparerSettings : ControllerSettings
{
    public List<DonutRawPreparerCapacity> capacities;

    public int GetCapacity(int level)
    {
        return capacities.Find(capacity => capacity.level == level).capacity;
    }

    public int GetCapacityUpgradePrice(int level)
    {
        return capacities.Find(speed => speed.level == level).upgradePrice;
    }
}

[System.Serializable]
public class DonutRawPreparerCapacity
{
    public int level;
    public int capacity;
    public int upgradePrice;
}
