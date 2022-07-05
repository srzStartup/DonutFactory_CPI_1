using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenInputManager : Singleton<ScreenInputManager>
{
    public bool touched { get; private set; }
    public Vector3 lastTouchPosition { get; private set; }
    private float screenTouchValueX;
    private float screenTouchValueY;

    public bool isListening = true;

    public float xPersentage => screenTouchValueX * 100.0f;
    public float yPersentage => screenTouchValueY * 100.0f;

    private void Update()
    {
        ListenTouches();
    }

    private void ListenTouches()
    {
        if (!isListening) return;

        if (touched)
        {
            screenTouchValueX = (Input.mousePosition.x - lastTouchPosition.x) / Screen.width;
            screenTouchValueY = (Input.mousePosition.y - lastTouchPosition.y) / Screen.height;
        }

        if (Input.GetMouseButton(0))
        {
            lastTouchPosition = Input.mousePosition;
            touched = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            lastTouchPosition = Vector3.zero;
            touched = false;
        }
    }
}
