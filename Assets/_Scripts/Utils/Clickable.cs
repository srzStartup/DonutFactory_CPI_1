using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Clickable : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    bool dragged = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragged = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!dragged)
            this.OnClick(eventData);

        dragged = false;
    }

    protected abstract void OnClick(PointerEventData eventData);
}
