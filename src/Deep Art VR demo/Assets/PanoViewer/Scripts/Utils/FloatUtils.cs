using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class FloatUtils
{
    public static IEnumerator Interpolate(float targetTime, float startValue, float endValue, UnityAction<float> action)
    {
        var lerpTime = 0.0f;

        while (lerpTime < targetTime)
        {
            lerpTime += Time.deltaTime;

            float percentage = lerpTime / targetTime;
            float finalValue = Mathf.Lerp(startValue, endValue, percentage);

            if (action != null)
            {
                action.Invoke(finalValue);
            }

            yield return null;
        }
    }
}
