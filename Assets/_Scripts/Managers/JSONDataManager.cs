using System.Collections;
using System.Collections.Generic;

using System.IO;

using UnityEngine;

[DefaultExecutionOrder(-9)]
public class JSONDataManager : Singleton<JSONDataManager>
{
    public string dataFileName;
    public bool refresh = false;
    private string _path;
    private DataModel _data;

    public DataModel data => _data;

    protected override void Awake()
    {
        base.Awake();

        _path = Path.Combine(Application.persistentDataPath, dataFileName + ".json");
        _data = new DataModel();

        if (refresh)
        {
            DeleteFile();
        }

        if (GetData() == null)
        {
            _data.totalMoney = 999999;
            _data.onboardingDone = true;
            _data.playerCapacityLevel = 3;
            _data.playerSpeedLevel = 3;

            // 0 3 6

            _data.setups = new List<SetupControllerModel>()
            {
                new SetupControllerModel(0, 1, 1, SetupControllerType.DonutRawPreparer, 0, true),
                new SetupControllerModel(1, 1, 1, SetupControllerType.DonutRawPreparer, 0, true),
                new SetupControllerModel(2, 1, 1, SetupControllerType.DonutRawPreparer, 0, true),
                new SetupControllerModel(3, 1, 1, SetupControllerType.Oven, 0, true),
                new SetupControllerModel(4, 1, 1, SetupControllerType.Oven, 0, true),
                new SetupControllerModel(5, 1, 1, SetupControllerType.Oven, 0, true),
                new SetupControllerModel(6, 1, 1, SetupControllerType.SauceSpiller, 0, true),
                new SetupControllerModel(7, 1, 1, SetupControllerType.SauceSpiller, 0, true),
                new SetupControllerModel(8, 1, 1, SetupControllerType.SauceSpiller, 0, true),
                new SetupControllerModel(9, 1, 1, SetupControllerType.CandySpiller, 0, true),
            };
        }

        SaveData();
    }

    public DataModel GetData()
    {
        if (!File.Exists(_path))
            return null;

        string json = File.ReadAllText(_path);
        _data = JsonUtility.FromJson<DataModel>(json);

        return _data;
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(_data, true);
        File.WriteAllText(_path, json);
    }

    public void DeleteFile()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}

[System.Serializable]
public class DataModel
{
    public int totalMoney;
    public bool onboardingDone;
    public List<SetupControllerModel> setups;
    public int playerSpeedLevel;
    public int playerCapacityLevel;
}

[System.Serializable]
public class SetupControllerModel
{
    public int id;
    public int capacityLevel;
    public int speedLevel;
    public SetupControllerType type;
    public int remainToUnlock;
    public bool isUnlocked;

    public SetupControllerModel(int id, int capacityLevel, int speedLevel, SetupControllerType type, int remainToUnlock, bool isUnlocked)
    {
        this.id = id;
        this.capacityLevel = capacityLevel;
        this.speedLevel = speedLevel;
        this.type = type;
        this.remainToUnlock = remainToUnlock;
        this.isUnlocked = isUnlocked;
    }
}
