using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StatueController : MonoBehaviour
{
    [SerializeField] private Material mat;

    int specularIntensityPropertyIndex;
    float initValue;

    void Awake()
    {
        initValue = mat.GetFloat("_SpecularIntensity");
        specularIntensityPropertyIndex = mat.shader.FindPropertyIndex("_SpecularIntensity");
    }

    void Start()
    {
        //DOVirtual.Float(1, 4, 2.5f, value => mat.SetFloat("_SpecularIntensity", value))
        //    .SetLoops(-1, LoopType.Yoyo)
        //    .SetEase(Ease.InBounce);
    }

    void OnDisable()
    {
        mat.SetFloat("_SpecularIntensity", initValue);
    }

    public void HandleUpgradeTrigger()
    {
        UIManager.Instance.OpenPlayerUpgradePanel();
    }
}
