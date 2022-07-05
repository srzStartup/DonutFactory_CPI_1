using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public Collectible retained { get; set; }
    public bool isEmpty => retained == null;

    public Collectible Release()
    {
        Collectible released = retained;
        retained = null;
        //released.transform.parent = null;

        return released;
    }
}
