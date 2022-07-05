using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public static class Utils
{
    private static readonly Dictionary<float, WaitForSeconds> waitMap = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds GetWaitForSeconds(float duration)
    {
        if (waitMap.TryGetValue(duration, out WaitForSeconds waitForSeconds))
        {
            return waitForSeconds;
        }

        waitMap.Add(duration, new WaitForSeconds(duration));

        return waitMap[duration];
    }

    public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element, Camera camera = null)
    {
        camera = camera != null ? camera : Camera.main;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, camera, out Vector3 worldPoint);

        return worldPoint;
    }

    private static PointerEventData _eventDataCurrentPosition;
    private static List<RaycastResult> _raycastResults;

    public static bool IsOverUI()
    {
        _eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        _raycastResults = new List<RaycastResult>();

        EventSystem.current.RaycastAll(_eventDataCurrentPosition, _raycastResults);

        return _raycastResults.Count > 0;
    }

    public static void WithPeriod(ref float nextTransactionTime, ref float transactionStartTime, float transactionPeriod, System.Action action)
    {
        if (nextTransactionTime <= transactionStartTime)
        {
            nextTransactionTime += transactionPeriod;

            action.Invoke();
        }

        transactionStartTime += Time.deltaTime;
    }

    public static void WithCooldown(float cooldown, ref float elapsedTime, System.Action action)
    {
        if (elapsedTime >= cooldown)
        {
            action?.Invoke();
            elapsedTime = 0;
        }
        else
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public static void WithCooldownPassOneParam<T>(float cooldown, ref float elapsedTime, T obj, System.Action<T> action)
    {
        if (elapsedTime >= cooldown)
        {
            action?.Invoke(obj);
            elapsedTime = 0;
        }
        else
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public static void WithCooldownPassTwoParam<T, K>(float cooldown, ref float elapsedTime, T obj1, K obj2, System.Action<T, K> action)
    {
        if (elapsedTime >= cooldown)
        {
            action?.Invoke(obj1, obj2);
            elapsedTime = 0;
        }
        else
        {
            elapsedTime += Time.deltaTime;
        }
    }
}
