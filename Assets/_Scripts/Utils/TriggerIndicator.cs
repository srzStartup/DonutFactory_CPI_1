using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerIndicator : MonoBehaviour
{
    [SerializeField] private InteractionIndicator[] interactionIndicators;

    #region Interaction Handlers

    public void Open()
    {
        foreach (InteractionIndicator interactionIndicator in interactionIndicators)
        {
            interactionIndicator.sprite.color = interactionIndicator.enabledColor;
        }
    }

    public void Close()
    {
        foreach (InteractionIndicator interactionIndicator in interactionIndicators)
        {
            interactionIndicator.sprite.color = interactionIndicator.disabledColor;
        }
    }

    #endregion
}

[System.Serializable]
public class InteractionIndicator
{
    public SpriteRenderer sprite;
    public Color enabledColor;
    public Color disabledColor;
}
