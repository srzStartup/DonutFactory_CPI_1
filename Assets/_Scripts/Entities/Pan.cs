using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    [SerializeField] private List<Transform> _slots;

    public List<Transform> slots => _slots;

    public Transform GetSlot()
    {
        return _slots.Find(someSlot => someSlot.childCount == 0);
    }

    public Transform GetSlot(int index)
    {
        return _slots[index];
    }

    public Collectible AsCollectible()
    {
        return GetComponent<Collectible>();
    }
}
