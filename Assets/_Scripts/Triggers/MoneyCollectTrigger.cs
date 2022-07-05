using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyCollectTrigger : MonoBehaviour
{
    [SerializeField] private ShowroomController showroom;

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if (showroom.monies.Count != 0)
            {
                player.CollectMoney(showroom.monies.Pop());
            }
        }
    }
}
