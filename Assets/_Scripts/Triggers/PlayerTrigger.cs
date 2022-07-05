using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private TriggerIndicator triggerIndicator;

    public PlayerTriggerEnterEvent playerTriggerEnterEvent;
    public PlayerTriggerStayEvent playerTriggerStayEvent;
    public PlayerTriggerExitEvent playerTriggerExitEvent;
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            triggerIndicator.Open();
            playerTriggerEnterEvent?.Invoke(player);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            playerTriggerStayEvent?.Invoke(player);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            triggerIndicator.Close();
            playerTriggerExitEvent?.Invoke(player);
        }
    }
}

[System.Serializable]
public class PlayerTriggerEnterEvent : UnityEvent<PlayerController>
{

}

[System.Serializable]
public class PlayerTriggerStayEvent : UnityEvent<PlayerController>
{

}

[System.Serializable]
public class PlayerTriggerExitEvent : UnityEvent<PlayerController>
{

}