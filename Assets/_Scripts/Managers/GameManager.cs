using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-5)]
public class GameManager : Singleton<GameManager>
{
    public int level { get; private set; } = 1;
    public bool started { get; private set; }
    public bool ended { get; private set; }
    public bool paused { get; private set; }
    public int currentMoney { get; private set; } = 0;
    public int multiplier = 1;

    [SerializeField] private InGameEventChannel _inGameEventChannel;

    public InGameEventChannel inGameEventChannel => _inGameEventChannel;

    void Start()
    {
        Input.multiTouchEnabled = false;
        currentMoney = JSONDataManager.Instance.data.totalMoney;
        PlayerController.Instance.speedLevel = JSONDataManager.Instance.data.playerSpeedLevel;
        PlayerController.Instance.capacityLevel = JSONDataManager.Instance.data.playerCapacityLevel;
        _inGameEventChannel.RaiseGameStartedEvent();
        StartLevel();
    }

    void OnEnable()
    {
        ElephantSDK.Elephant.LevelStarted(1);
    }

    public void StartLevel()
    {
        started = true;
        _inGameEventChannel.RaiseLevelStartedEvent();
    }

    public void EndLevel(bool success = true)
    {
        ended = true;
    }

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    public void UpdateMoney(int amount)
    {
        currentMoney += amount;
        JSONDataManager.Instance.data.totalMoney = currentMoney;
        JSONDataManager.Instance.SaveData();

        _inGameEventChannel.RaiseMoneyUpdatedEvent(amount);
    }

    public bool TryUpdateMoney(int amount)
    {

        if (currentMoney < amount)
        {
            return false;
        }
        else
        {
            currentMoney += amount;

            _inGameEventChannel.RaiseMoneyUpdatedEvent(amount);

            return true;
        }
    }

    private void OnDisable()
    {
        JSONDataManager.Instance.SaveData();
    }
}
