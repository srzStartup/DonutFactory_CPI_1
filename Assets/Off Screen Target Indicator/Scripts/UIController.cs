using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Canvas canvas;

    public List<TargetIndicator> targetIndicators = new List<TargetIndicator>();

    public Camera MainCamera;

    public GameObject TargetIndicatorPrefab;

    public Transform indicatorParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(targetIndicators.Count > 0)
        {
            for(int i = 0; i < targetIndicators.Count; i++)
            {
                targetIndicators[i].UpdateTargetIndicator();
            }
        }
    }

    public TargetIndicator AddTargetIndicator(GameObject target, Color color, Sprite inScreenImage, Sprite offScreenImage)
    {
        TargetIndicator indicator = Instantiate(TargetIndicatorPrefab, canvas.transform).GetComponent<TargetIndicator>();
        indicator.InitialiseTargetIndicator(target, MainCamera, canvas);
        indicator.SetImageAndColors(color, inScreenImage, offScreenImage);
        //indicator.transform.SetParent(indicatorParent, false);
        targetIndicators.Add(indicator);

        return indicator;
    }

    public void ChangeIndicatorTarget(TargetIndicator indicator, GameObject target)
    {
        indicator.InitialiseTargetIndicator(target, MainCamera, canvas);
    }
}
