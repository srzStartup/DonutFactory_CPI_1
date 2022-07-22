using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CandySpillerCamTrigger : MonoBehaviour
{
    public CandySpillerCamTriggerEnterEvent candySpillerCamTriggerEnterEvent;
    public CandySpillerCamTriggerExitEvent candySpillerCamTriggerExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            candySpillerCamTriggerEnterEvent?.Invoke(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            candySpillerCamTriggerExitEvent?.Invoke(player);
        }
    }
}

[System.Serializable]
public class CandySpillerCamTriggerEnterEvent : UnityEvent<PlayerController>
{

}

[System.Serializable]
public class CandySpillerCamTriggerExitEvent : UnityEvent<PlayerController>
{

}
