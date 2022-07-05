using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GarbageCan : MonoBehaviour
{
    [SerializeField] private TriggerIndicator triggerIndicator;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform core;

    float collectCooldown = .2f;
    float elapsedTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            triggerIndicator.Open();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player) && player.state == PlayerState.Idle)
        {
            if (elapsedTime >= collectCooldown)
            {
                Sequence sequence = DOTween.Sequence();
                float animationLifetime = .25f;
                float timePosition = 0f;
                int index = 0;

                Collectible collectible = player.DetachItem(CollectibleType.Any, core.position).Item1;

                if (collectible)
                {
                    Tween toTrashTween = collectible.transform.DOMove(core.position, animationLifetime)
                        .SetEase(Ease.Flash)
                        .OnStart(() => collectible.transform.parent = null)
                        .OnComplete(() =>
                        {
                            if (collectible.type == CollectibleType.Pan || collectible.type == CollectibleType.PanWithBakedDonuts || collectible.type == CollectibleType.PanWithRawDonuts)
                            {
                                Pan pan = collectible.GetComponent<Pan>();

                                foreach (Transform slot in pan.slots)
                                {
                                    Transform donutTransform = slot.GetChild(0);
                                    if (donutTransform)
                                    {
                                        donutTransform.parent = null;

                                        donutTransform.Find("DonutRaw").gameObject.SetActive(true);
                                        donutTransform.Find("DonutBaked").gameObject.SetActive(false);

                                        Collectible donut = donutTransform.GetComponent<Collectible>();
                                        ObjectPooler.Instance.PushToQueue(donut.objectPoolTag, donut.gameObject);
                                    }
                                }
                            }

                        ObjectPooler.Instance.PushToQueue(collectible.objectPoolTag, collectible.gameObject);
                    });

                    timePosition = animationLifetime * ((float)index++ / player.stackManager.Count);

                    sequence.Insert(timePosition, toTrashTween);
                }


                elapsedTime = 0f;
            }
            else
            {
                elapsedTime += Time.deltaTime;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            triggerIndicator.Close();

            elapsedTime = 0f;
        }
    }
}
