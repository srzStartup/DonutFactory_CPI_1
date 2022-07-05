using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/In Game Event Channel")]
public class InGameEventChannel : ScriptableObject
{
    // Game states
    public UnityAction GameStartedEvent;
    public UnityAction LevelStartedEvent;
    public UnityAction LevelAccomplishedEvent;
    public UnityAction LevelFailedEvent;

    // player events
    public UnityAction<PlayerState> PlayerStateChangedEvent;
    public UnityAction<PlayerTaskState> PlayerTaskStateChangedEvent;

    public UnityAction<int> MoneyUpdatedEvent;

    public UnityAction PasteConsumeByDonutRawPreparerEvent;
    public UnityAction PanWithRawDonutsReadyEvent;
    public UnityAction PanWithRawDonutsConsumeByOvenEvent;
    public UnityAction PanWithBakedDonutsReadyEvent;
    public UnityAction PanWithBakedDonutsConsumeBySauceSpillerEvent;
    public UnityAction SaucedDonutReadyEvent;
    public UnityAction SaucedDonutConsumeByShowroomEvent;

    public UnityAction SendingRawDonutsToPanSequenceStartEvent;

    public void RaiseGameStartedEvent()
    {
        GameStartedEvent?.Invoke();
    }

    public void RaiseLevelStartedEvent()
    {
        LevelStartedEvent?.Invoke();
    }

    public void RaiseLevelAccomplishedEvent()
    {
        LevelAccomplishedEvent?.Invoke();
    }

    public void RaiseLevelFailedEvent()
    {
        LevelFailedEvent?.Invoke();
    }

    public void RaisePlayerStateChangedEvent(PlayerState state)
    {
        PlayerStateChangedEvent?.Invoke(state);
    }

    public void RaisePlayerTaskStateChangedEvent(PlayerTaskState taskState)
    {
        PlayerTaskStateChangedEvent?.Invoke(taskState);
    }

    public void RaiseMoneyUpdatedEvent(int updateAmount)
    {
        MoneyUpdatedEvent?.Invoke(updateAmount);
    }

    public void RaisePasteConsumeByDonutRawPreparerEvent()
    {
        PasteConsumeByDonutRawPreparerEvent?.Invoke();
    }

    public void RaisePanWithRawDonutsReadyEvent()
    {
        PanWithRawDonutsReadyEvent?.Invoke();
    }

    public void RaisePanWithRawDonutsConsumeByOvenEvent()
    {
        PanWithRawDonutsConsumeByOvenEvent?.Invoke();
    }

    public void RaisePanWithBakedDonutsReadyEvent()
    {
        PanWithBakedDonutsReadyEvent?.Invoke();
    }

    public void RaisePanWithBakedDonutsConsumeBySauceSpillerEvent()
    {
        PanWithBakedDonutsConsumeBySauceSpillerEvent?.Invoke();
    }

    public void RaiseSaucedDonutReadyEvent()
    {
        SaucedDonutReadyEvent?.Invoke();
    }

    public void RaiseSaucedDonutConsumeByShowroomEvent()
    {
        SaucedDonutConsumeByShowroomEvent?.Invoke();
    }

    public void RaiseSendingRawDonutsToPanSequenceStartEvent()
    {
        SendingRawDonutsToPanSequenceStartEvent?.Invoke();
    }
}
