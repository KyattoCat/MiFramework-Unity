using System.Collections;
using UnityEngine;

namespace MiFramework.Tween
{
    public static class MiTween
    {
        private const float FLOAT_EPSILON = 1E-3f;
        private struct TweenCache
        {
            public float startTime;
        }

        private static IEnumerator DoMoveLocalYInternal(RectTransform rectTransform, float startY, float endY, float duration)
        {
            TweenCache cache = new TweenCache() { startTime = Time.time };
            Vector2 position = rectTransform.localPosition;
            position.y = startY;
            rectTransform.localPosition = position;
            while (Mathf.Abs(rectTransform.localPosition.y - endY) > FLOAT_EPSILON)
            {
                yield return new WaitForEndOfFrame();
                position = rectTransform.localPosition;
                position.y = Mathf.Lerp(startY, endY, (Time.time - cache.startTime) / duration);
                rectTransform.localPosition = position;
            }
            rectTransform.localPosition = new Vector2(rectTransform.localPosition.x, endY);
        }

        public static void DoMoveLocalY(this RectTransform rectTransform, float startY, float endY, float duration)
        {
            Debug.Log($"[DoMoveY] {rectTransform.name} startY = {startY} endY = {endY} duration = {duration}");
            CoroutineManager.Instance.StartCoroutine(DoMoveLocalYInternal(rectTransform, startY, endY, duration));
        }
    }

}