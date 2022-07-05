using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SauceTrigger : MonoBehaviour
{
    [SerializeField] private DonutConvertType donutConvertType;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Collectible collectible) && collectible.type == CollectibleType.DonutSauced)
        {
            switch (donutConvertType)
            {
                case DonutConvertType.Chocolate:

                    collectible.type = CollectibleType.DonutSaucedChocolate;
                    collectible.transform.Find("DonutBakedChoco").gameObject.SetActive(true);

                    break;
                case DonutConvertType.Strawberry:

                    collectible.type = CollectibleType.DonutSaucedStrawberry;
                    collectible.transform.Find("DonutBakedStrawberry").gameObject.SetActive(true);

                    break;
                case DonutConvertType.Banana:

                    collectible.type = CollectibleType.DonutSaucedCaramel;
                    collectible.transform.Find("DonutBakedBanana").gameObject.SetActive(true);

                    break;
            }
        }
    }
}

public enum DonutConvertType
{
    Chocolate,
    Strawberry,
    Banana
}
