using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public static class Erp {
        public delegate void OnTweenFrame(float value);

        public static float Interpolate(Interpolator.Type type, float a, float b, float pct) {
            Interpolator i = new Interpolator(type);
            return i.GetValue(a, b, pct);
        }

        public static float MoveTowards(this float a, float b, float dampen) {
            return Interpolate(Interpolator.Type.Linear, a, b, dampen);
        }

        public static IEnumerator Tween(Interpolator.Type type, float start, float end, float duration, OnTweenFrame callback) {
            float elapsed = 0;

            while (elapsed < duration) {
                float value = Interpolate(type, start, end, elapsed / duration);
                callback(value);
                elapsed += TimeManager.DeltaTime;
                yield return null;
            }
        }
    }
}