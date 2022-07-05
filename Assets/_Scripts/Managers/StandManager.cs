using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class StandManager : Singleton<StandManager>
{
    public List<ISetupController> setups { get; private set; }

    public int unlockedCount;

    protected override void Awake()
    {
        base.Awake();

        setups = new List<ISetupController>();

        int index = 0;

        foreach (ISetupController setup in GetComponentsInChildren<ISetupController>())
        {
            setup.id = index++;
            setups.Add(setup);
        }

        JSONDataManager.Instance.data.setups.ForEach(setupData =>
        {
            ISetupController setup = Get(setupData.id);

            switch (setupData.type)
            {
                case SetupControllerType.DonutRawPreparer:

                    DonutRawPreparerController donutRawPreparer = (DonutRawPreparerController)setup;

                    donutRawPreparer.capacityLevel = setupData.capacityLevel;

                    break;
                case SetupControllerType.Oven:

                    OvenSetupControllerNEW oven = (OvenSetupControllerNEW)setup;

                    oven.capacityLevel = setupData.capacityLevel;
                    oven.cookingTimeLevel = setupData.speedLevel;

                    break;
                case SetupControllerType.SauceSpiller:
                    break;
                case SetupControllerType.CandySpiller:
                    break;
            }

            setup.IsUnlocked = setupData.isUnlocked;
            setup.remainToUnlock = setupData.remainToUnlock == -1 ? setup.Settings.unlockPrice : setupData.remainToUnlock;

            if (setup.IsUnlocked)
            {
                unlockedCount++;
            }
        });
    }

    public List<T> GetListOfType<T>(SetupControllerType type)
    {
        return setups.FindAll(setup => setup.type == type).Cast<T>().ToList();
    }

    public T FindOfType<T>(SetupControllerType type)
    {
        return setups.FindAll(setup => setup.type == type).Cast<T>().First();
    }

    public ISetupController Get(int id)
    {
        return setups[id] ?? setups.Find(setup => setup.id == id);
    }
}
