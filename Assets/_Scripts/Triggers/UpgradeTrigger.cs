using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class UpgradeTrigger : MonoBehaviour
{
    public UnityEvent UpgradeTriggerEnter;

    [SerializeField] private SpriteRenderer filler;

    Tween currentTween;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            currentTween = DOVirtual.Float(0, 360f, 1.75f, value => filler.material.SetFloat("_Arc1", value));
            currentTween.SetEase(Ease.Linear).OnComplete(() => UpgradeTriggerEnter?.Invoke());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController _))
        {
            filler.material.SetFloat("_Arc1", 0f);
            if (currentTween != null)
            {
                currentTween.Kill();
                currentTween = null;
            }

            UIManager.Instance.CloseUpgradePanel();
        }
    }
}
