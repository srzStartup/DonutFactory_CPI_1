using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShowroomController : MonoBehaviour
{
    [SerializeField] private List<Transform> _donutSlots;
    [SerializeField] private List<Transform> _moneySlots;

    [SerializeField] private TriggerIndicator triggerIndicator;

    Queue<Transform> _donutSlotQueue;
    Queue<Transform> _moneySlotQueue;
    Stack<Collectible> _donuts;
    Stack<Money> _monies;
    public Stack<Collectible> donuts => _donuts;
    public Stack<Money> monies => _monies;

    float detachCooldown = .05f;
    float elapsedTime;

    Transform donutSlotCurrentlyUsed;

    void Start()
    {
        _donutSlotQueue = new Queue<Transform>();
        _moneySlotQueue = new Queue<Transform>();
        _donuts = new Stack<Collectible>();
        _monies = new Stack<Money>();

        _donutSlots.ForEach(slot => _donutSlotQueue.Enqueue(slot));
        _moneySlots.ForEach(slot => _moneySlotQueue.Enqueue(slot));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Open();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player) && player.state == PlayerState.Idle)
        {
            Utils.WithCooldownPassOneParam(detachCooldown, ref elapsedTime, player, DetachItemFromPlayer);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Close();
        }
    }

    void DetachItemFromPlayer(PlayerController player)
    {
        Transform slot = _donutSlotQueue.Peek();
        donutSlotCurrentlyUsed = slot;

        Vector3 slotPosition = CalculateDonutSlotPosition();

        player.DetachItem(CollectibleType.DonutSauced, slotPosition, slot, onStart: OnDetachItemStart, onComplete: OnDetachItemComplete);
    }

    void OnDetachItemStart(Collectible collectible, Sequence sequence)
    {
        _donutSlotQueue.Dequeue();
        _donutSlotQueue.Enqueue(donutSlotCurrentlyUsed);

        GameManager.Instance.inGameEventChannel.RaiseSaucedDonutConsumeByShowroomEvent();
    }

    void OnDetachItemComplete(Collectible collectible, Sequence sequence)
    {
        _donuts.Push(collectible);
    }

    Vector3 CalculateDonutSlotPosition()
    {
        Vector3 slotPosition = donutSlotCurrentlyUsed.position;

        if (donutSlotCurrentlyUsed.childCount != 0)
        {
            Collectible collectible = donutSlotCurrentlyUsed.GetComponentInChildren<Collectible>();
            float unitHeight = donutSlotCurrentlyUsed.InverseTransformPoint(collectible.topPoint.position).y - donutSlotCurrentlyUsed.InverseTransformPoint(collectible.transform.position).y;
            float height = unitHeight * donutSlotCurrentlyUsed.childCount;

            slotPosition = donutSlotCurrentlyUsed.TransformPoint(new Vector3(0f, height, 0f));
        }

        return slotPosition;
    }

    Vector3 CalculateMoneySlotPosition(Transform moneySlot)
    {
        Vector3 slotPosition = moneySlot.position;

        if (moneySlot.childCount != 0)
        {
            Money money = moneySlot.GetComponentInChildren<Money>();
            float unitHeight = moneySlot.InverseTransformPoint(money.topPoint.position).y - moneySlot.InverseTransformPoint(money.transform.position).y;
            float height = unitHeight * moneySlot.childCount;

            slotPosition = moneySlot.TransformPoint(new Vector3(0f, height, 0f));
        }

        return slotPosition;
    }

    public void GetPaid(CustomerController customer, int totalAmount, System.Action onStart = null, System.Action onComplete = null)
    {
        float animationLifetimePerTween = .01f;
        float totalAnimationLifetime = animationLifetimePerTween * totalAmount;
        int index = 0;
        float timePosition = 0f;
        Sequence sequence = DOTween.Sequence();

        int moneyCount = totalAmount / 2;
        int remainder = totalAmount % 2;
        for (int i = 0; i < moneyCount; i++)
        {
            Transform nextMoneySlot = _moneySlotQueue.Dequeue();
            _moneySlotQueue.Enqueue(nextMoneySlot);
            Vector3 spawnPosition = customer.transform.position;
            spawnPosition.y += 1f;

            GameObject moneyGO = ObjectPooler.Instance.SpawnFromPool("money", spawnPosition, Quaternion.identity);
            Money money = moneyGO.GetComponent<Money>();
            money.worth = 5;
            moneyGO.transform.parent = nextMoneySlot;
            if (i == moneyCount - 1)
            {
                money.worth += remainder;
            }
            _monies.Push(money);

            Tween moneyJumpTween = money.transform.DOJump(CalculateMoneySlotPosition(nextMoneySlot), 2, 1, totalAnimationLifetime);

            timePosition = totalAnimationLifetime * ((float)index++ / totalAmount);
            sequence.Insert(timePosition, moneyJumpTween);
        }

        sequence.OnStart(() => onStart?.Invoke())
            .OnComplete(() => onComplete?.Invoke());
    }
}
