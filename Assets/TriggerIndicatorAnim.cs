using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TriggerIndicatorAnim : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] rectangleSprites;

    Sequence sequence;

    public void AnimateTextBox()
    {
        float animationLifetime = 1f;
        float delay = .5f;

        float timePosition = 0f;
        int index = 0;

        float startAlphaValue = .59f;
        float endAlphaValue = .0f;

        Vector3 endScaleValue = Vector3.one * 1f;


        sequence = DOTween.Sequence();

        foreach (SpriteRenderer rectangleSprite in rectangleSprites)
        {
            Tween fadeTween = DOVirtual.Float(startAlphaValue, endAlphaValue, animationLifetime, (floatValue) =>
            {
                Color color = rectangleSprite.color;
                color.a = floatValue;

                rectangleSprite.color = color;
            });

            Tween scaleTween = rectangleSprite.transform
                .DOScale(endScaleValue, animationLifetime)
                .SetEase(Ease.Linear);

            timePosition = animationLifetime * ((float)index++ / rectangleSprites.Length);

            sequence.Insert(timePosition, scaleTween)
                    .Insert(timePosition, fadeTween);
        }

        sequence.SetLoops(-1)
            .SetDelay(delay)
            .OnStepComplete(() =>
            {
                foreach (SpriteRenderer rectangleSprite in rectangleSprites)
                {
                    Color color = rectangleSprite.color;
                    color.a = .0f;

                    rectangleSprite.color = color;
                }
            });
    }

    public void StopAnim()
    {
        sequence.Kill(false);
    }
}
