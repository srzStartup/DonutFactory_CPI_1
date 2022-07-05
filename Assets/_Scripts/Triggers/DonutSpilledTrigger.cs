using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DonutSpilledTrigger : MonoBehaviour
{
    public DonutSpilledTriggerEnter donutSpilledTriggerEnter;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Collectible collectible) && CheckCollectible(collectible))
        {
            donutSpilledTriggerEnter?.Invoke(collectible);
        }
    }

    bool CheckCollectible(Collectible collectible)
    {
        return collectible.type == CollectibleType.DonutWithBonbon || collectible.type == CollectibleType.DonutWithSprinkles || collectible.type == CollectibleType.DonutWithOreo;
    }
}

[System.Serializable]
public class DonutSpilledTriggerEnter : UnityEvent<Collectible>
{

}
