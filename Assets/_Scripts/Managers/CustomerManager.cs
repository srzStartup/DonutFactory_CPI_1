using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CustomerManager : Singleton<CustomerManager>
{
    [SerializeField] private ShowroomController showroom;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] leavingPath;
    [SerializeField] private List<Transform> _slots;
    [SerializeField] private Transform customersParent;

    CustomerController currentCustomer;

    Queue<CustomerController> customerQueue;

    Vector3[] leavingPathVectors;

    // customer stack settings
    float collectCooldown = .05f;
    float elapsedTime;

    public int orderMaxCount;
    public int orderMinCount;

    void Start()
    {
        customerQueue = new Queue<CustomerController>();

        if (StandManager.Instance.unlockedCount > 3 && StandManager.Instance.unlockedCount < 5)
        {
            orderMaxCount++;
        }

        else if (StandManager.Instance.unlockedCount >= 5 && StandManager.Instance.unlockedCount < 7)
        {
            orderMaxCount += 2;
            orderMinCount++;
        }

        else if (StandManager.Instance.unlockedCount >= 7)
        {
            orderMaxCount += 5;
            orderMinCount += 2;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            Transform slot = _slots[i];

            CustomerController customer = SpawnCustomer(slot.position, slot.rotation.eulerAngles, parent: slot);
            customerQueue.Enqueue(customer);
        }

        List<Vector3> leavingPathVectorList = new List<Vector3>();

        foreach (Transform pathPoint in leavingPath)
        {
            leavingPathVectorList.Add(pathPoint.position);
        }

        leavingPathVectors = leavingPathVectorList.ToArray();
    }

    void LateUpdate()
    {
        CustomerController nextCustomer = customerQueue.Peek();

        if (showroom.donuts.Count >= nextCustomer.orderCount && !currentCustomer)
        {
            currentCustomer = customerQueue.Dequeue();
            currentCustomer.transform.parent = null;

            currentCustomer.Move(transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CustomerController customer) && currentCustomer == customer)
        {
            for (int i = 0; i < customerQueue.Count; i++)
            {
                CustomerController nextCustomer = customerQueue.Dequeue();
                nextCustomer.transform.parent = _slots[i];
                nextCustomer.Move(_slots[i].position);
                customerQueue.Enqueue(nextCustomer);
            }

            Transform lastSlot = _slots[_slots.Count - 1];
            CustomerController newCustomer = SpawnCustomer(lastSlot.position, lastSlot.rotation.eulerAngles, parent: lastSlot);
            customerQueue.Enqueue(newCustomer);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out CustomerController customer) && currentCustomer == customer)
        {
            if (customer.collectedCount < customer.orderCount)
            {
                Utils.WithCooldownPassOneParam(collectCooldown, ref elapsedTime, customer, MakeCustomerCollect);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CustomerController customer) && currentCustomer == customer)
        {
            elapsedTime = 0f;

            currentCustomer = null;
        }
    }

    void MakeCustomerCollect(CustomerController customer)
    {
        customer.Collect(showroom.donuts.Pop(), onComplete: () => 
        {
            if (customer.collectedCount == customer.orderCount)
            {
                showroom.GetPaid(customer, customer.totalAmount, onComplete: () => 
                    customer.MoveOnPath(leavingPathVectors, onComplete: () =>
                    {
                        foreach (Collectible collectible in customer.stackManager.items)
                        {
                            ObjectPooler.Instance.PushToQueue(collectible.objectPoolTag, collectible.gameObject);
                        }

                        customer.stackManager.Clear();

                        ObjectPooler.Instance.PushToQueue("customer", customer.gameObject);
                    }));
            }
        });
    }

    CustomerController SpawnCustomer(Vector3? spawnPosition = null, Vector3? spawnRotation = null, int orderCount = 1, Transform parent = null)
    {
        Vector3 spawnPos = spawnPosition == null ? Vector3.zero : (Vector3)spawnPosition;
        Vector3 spawnRot = spawnRotation == null ? Vector3.zero : (Vector3)spawnRotation;

        GameObject customerGO = ObjectPooler.Instance.SpawnFromPool("customer", spawnPos, Quaternion.Euler(spawnRot));
        customerGO.transform.parent = parent;
        CustomerController customer = customerGO.GetComponent<CustomerController>();
        customer.orderCount = Random.Range(orderMinCount, orderMaxCount);

        return customer;
    }
}
