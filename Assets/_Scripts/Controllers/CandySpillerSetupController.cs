using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CandySpillerSetupController : SetupController<CandySpillerSettings>
{
    public override SetupControllerType type => SetupControllerType.CandySpiller;

    [SerializeField] private Collider[] triggers;

    [SerializeField] private Transform machineTransform;
    [SerializeField] private List<Transform> _tableSlots;
    [SerializeField] private Transform conveyorLike;
    [SerializeField] private Transform candy1StartPoint;
    [SerializeField] private Transform candy2StartPoint;
    [SerializeField] private Transform oreoStartPoint;

    [SerializeField] private ParticleSystem bonbonParticle;
    [SerializeField] private ParticleSystem sprinklesParticle;
    [SerializeField] private ParticleSystem oreoParticle;

    Queue<Transform> tableSlotQueue;
    Stack<Collectible> readyDonuts;

    float detachCooldown = .25f;
    float elapsedTime_DETACH;
    float collectCooldown = .05f;
    float elapsedTime_COLLECT;

    protected override void Start()
    {
        base.Start();
        tableSlotQueue = new Queue<Transform>();
        readyDonuts = new Stack<Collectible>();

        _tableSlots.ForEach(tableSlot => tableSlotQueue.Enqueue(tableSlot));
    }

    void Update()
    {
        conveyorLike.Rotate(Vector3.down, .3f);
    }

    public void OnPlayerTriggerStayCandy1(PlayerController player)
    {
        if (player.state == PlayerState.Idle)
        {
            Utils.WithCooldownPassTwoParam(detachCooldown, ref elapsedTime_DETACH, player, TriggerCandyType.Bonbon, DetachItemFromPlayer);
        }
    }

    public void OnPlayerTriggerExitCandy1(PlayerController _)
    {
        elapsedTime_DETACH = 0f;
    }

    public void OnPlayerTriggerStayCandy2(PlayerController player)
    {
        if (player.state == PlayerState.Idle)
        {
            Utils.WithCooldownPassTwoParam(detachCooldown, ref elapsedTime_DETACH, player, TriggerCandyType.Sprinkles, DetachItemFromPlayer);
        }
    }

    public void OnPlayerTriggerExitCandy2(PlayerController _)
    {
        elapsedTime_DETACH = 0f;
    }

    public void OnPlayerTriggerStayOreo(PlayerController player)
    {
        if (player.state == PlayerState.Idle)
        {
            Utils.WithCooldownPassTwoParam(detachCooldown, ref elapsedTime_DETACH, player, TriggerCandyType.Oreo, DetachItemFromPlayer);
        }
    }

    public void OnPlayerTriggerExitOreo(PlayerController _)
    {
        elapsedTime_DETACH = 0f;
    }

    void DetachItemFromPlayer(PlayerController player, TriggerCandyType type)
    {
        ParticleSystem particle = null;
        string candy = "";
        Vector3 pos = Vector3.zero;
        CollectibleType collectibleType = CollectibleType.DonutSauced;

        switch (type)
        {
            case TriggerCandyType.Bonbon:
                collectibleType = CollectibleType.DonutWithBonbon;
                particle = bonbonParticle;
                candy = "Candy1";
                pos = candy2StartPoint.position;
                break;
            case TriggerCandyType.Sprinkles:
                collectibleType = CollectibleType.DonutWithSprinkles;
                particle = sprinklesParticle;
                candy = "Candy2";
                pos = candy1StartPoint.position;
                break;
            case TriggerCandyType.Oreo:
                collectibleType = CollectibleType.DonutWithOreo;
                particle = oreoParticle;
                candy = "Oreo";
                pos = oreoStartPoint.position;
                break;
        }

        player.DetachItem(CollectibleType.DonutSaucedWithoutCandy, pos,
            onStart: (collectible, sequence) => particle.Play(),
            onComplete: (collectible, sequence) =>
            {
                collectible.transform.Find(candy).gameObject.SetActive(true);
                collectible.type = collectibleType;
                particle.Stop();
                collectible.transform.parent = conveyorLike;
                collectible.worth *= 2;
            });
    }

    public void OnPlayerTriggerEnterToCollect(PlayerController player)
    {
        if (player.state == PlayerState.Idle)
        {
            if (readyDonuts.Count != 0 && player.CanTake(1))
            {
                if (elapsedTime_COLLECT >= collectCooldown)
                {
                    Collectible donut = readyDonuts.Pop();
                    player.Collect(donut);
                    elapsedTime_COLLECT = 0;
                }
                else
                {
                    elapsedTime_COLLECT += Time.deltaTime;
                }
            }
        }
    }

    public void OnPlayerTriggerExitToCollect(PlayerController player)
    {
        elapsedTime_COLLECT = 0f;
    }

    public void OnDonutSpilledTriggerEnter(Collectible collectible)
    {
        Transform nextSlot = tableSlotQueue.Dequeue();
        Vector3 slotPos = nextSlot.position;
        collectible.transform.parent = nextSlot;
        slotPos.y += (nextSlot.InverseTransformPoint(new Vector3(0f, collectible.topPoint.position.y, 0f)).y) * nextSlot.childCount;

        collectible.transform.DOMove(slotPos, .25f)
            .OnComplete(() => readyDonuts.Push(collectible));
        tableSlotQueue.Enqueue(nextSlot);
    }

    protected override void OnUnlock()
    {
        machineTransform.DORewind();
        machineTransform.DOPunchScale(Vector3.one * .1f, .5f)
            .OnComplete(() =>
            {
                foreach (Collider trigger in triggers)
                {
                    trigger.enabled = true;
                }
            });
    }
}

public enum TriggerCandyType
{
    None,
    Oreo,
    Bonbon,
    Sprinkles,
}
