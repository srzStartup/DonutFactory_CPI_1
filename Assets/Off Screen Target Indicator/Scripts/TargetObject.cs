using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetObject : MonoBehaviour
{
    //public UIManager ui;
    public Color color = Color.red;
    public Sprite inScreenImage, offScreenImage;
    public bool isActive = false;
    private void Start()
    {
        if (isActive)
        {
            UIManager.Instance.AddTargetIndicator(gameObject, color, inScreenImage, offScreenImage);
        }
    }

    public void OpenUI()
    {
        UIManager.Instance.AddTargetIndicator(gameObject, color, inScreenImage, offScreenImage);
    }
}
