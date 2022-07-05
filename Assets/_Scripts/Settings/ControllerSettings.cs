using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSettings : ScriptableObject
{
    public List<MachineFeature> features;

    public int level;
    public int unlockPrice;
    public int upgradePrice;
    public int maxLevel;

    public virtual int CalculateNextUpgradePrice()
    {
        return upgradePrice * 2;
    }

    public void LevelUp()
    {
        if (maxLevel == level)
        {
            return;
        }

        level++;
    }

    public virtual int GetUpgradePrice()
    {
        return level * upgradePrice;
    }

    void OnDisable()
    {
        level = 1;
    }
}

[System.Serializable]
public class MachineFeature
{
    public int level;
    public int upgradePrice;
    public int maxLevel;

    public virtual int GetUpgradePrice()
    {
        return level * upgradePrice;
    }

    public void LevelUp()
    {
        if (maxLevel == level)
        {
            return;
        }

        level++;
    }
}
