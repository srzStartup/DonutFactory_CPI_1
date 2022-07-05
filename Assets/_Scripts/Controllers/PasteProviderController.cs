using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PasteProviderController : MonoBehaviour
{
    [SerializeField] private TriggerIndicator triggerIndicator;
    [SerializeField] private List<Transform> pasteSlots;

    private Stack<Collectible> pastes;

    #region paste spawn handler

    public float spawnPeriod = 5.0f;
    private float nextSpawnTime;
    private float spawnStartTime;

    private float collectCooldown = .05f;
    private float elapsedTime;

    #endregion

    void Start()
    {
        pastes = new Stack<Collectible>();
        nextSpawnTime += spawnPeriod;
    }

    void FixedUpdate()
    {
        HandlePasteSpawn();
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
            if (pastes.Count != 0 && player.CanTake(pastes.Peek().holds))
            {
                Utils.WithCooldownPassOneParam(collectCooldown, ref elapsedTime, player, MakePlayerCollect);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            triggerIndicator.Close();
            elapsedTime = 0;
        }
    }

    void MakePlayerCollect(PlayerController player)
    {
        player.Collect(pastes.Pop());
    }

    void HandlePasteSpawn()
    {
        if (nextSpawnTime <= spawnStartTime)
        {
            nextSpawnTime += spawnPeriod;

            Transform slot = pasteSlots.Find((slot) => slot.childCount == 0);

            if (slot)
            {
                GameObject pasteGO = ObjectPooler.Instance.SpawnFromPool("paste", slot.position, Quaternion.identity);

                if (pasteGO)
                {
                    Transform pasteTransform = pasteGO.transform;

                    pasteTransform.DOPunchScale(Vector3.one * .25f, .5f)
                        .OnComplete(() =>
                        {
                            Collectible paste = pasteGO.GetComponent<Collectible>();
                            pasteTransform.parent = slot;
                            pastes.Push(paste);
                        });
                }
            }
        }

        spawnStartTime += Time.deltaTime;
    }
}
