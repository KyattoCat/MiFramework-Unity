using UnityEngine;

public static class UnityExtensions
{
    public static T SafeGetComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }
    public static T SafeGetComponent<T>(this RectTransform rectTransform) where T : Component
    {
        T component = rectTransform.GetComponent<T>();
        if (component == null)
            component = rectTransform.gameObject.AddComponent<T>();
        return component;
    }

    public static void SetActiveUI(this GameObject gameObject, bool bShow)
    {
        CanvasGroup canvasGroup = gameObject.SafeGetComponent<CanvasGroup>();
        UpdateCanvasGroup(canvasGroup, bShow);
    }

    public static void SetActiveUI(this RectTransform rectTransform, bool bShow)
    {
        CanvasGroup canvasGroup = rectTransform.SafeGetComponent<CanvasGroup>();
        UpdateCanvasGroup(canvasGroup, bShow);
    }

    private static void UpdateCanvasGroup(CanvasGroup canvasGroup, bool bShow)
    {
        canvasGroup.alpha = bShow ? 1 : 0;
        canvasGroup.blocksRaycasts = bShow;
        canvasGroup.interactable = bShow;
    }
}