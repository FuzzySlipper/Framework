using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class Interpolator {
        public enum Type {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInQuart,
            EaseOutQuart,
            EaseInOutQuart,
            EaseInQuint,
            EaseOutQuint,
            EaseInOutQuint,
            EaseInSine,
            EaseOutSine,
            EaseInOutSine,
            EaseInExpo,
            EaseOutExpo,
            EaseInOutExpo,
            EaseInCirc,
            EaseOutCirc,
            EaseInOutCirc,
            EaseInElastic,
            EaseOutElastic,
            EaseInBack,
            EaseOutBack,
        }

        public Type type;

        public Interpolator(Type type) {
            this.type = type;
        }

        public float GetValue(float start, float end, float percentage) {
            float b = start;
            float c = end - start;
            float t = percentage;

            switch (type) {
                case Type.Linear:
                    return Mathf.Lerp(start, end, percentage);
                case Type.EaseInQuad:
                    return c * t * t + b;
                case Type.EaseOutQuad:
                    return -c * (t) * (t - 2) + b;
                case Type.EaseInOutQuad:
                    if ((t / 2) < 1) {
                        return c / 2 * t * t + b;
                    }

                    return -c / 2 * ((t - 1) * (t - 3) - 1) + b;
                case Type.EaseInCubic:
                    return c * (t) * t * t + b;
                case Type.EaseOutCubic:
                    t -= 1;
                    return c * (t * t * t + 1) + b;
                case Type.EaseInOutCubic:
                    if ((t / 2) < 1) {
                        return c / 2 * t * t * t + b;
                    }

                    t -= 2;
                    return c / 2 * (t * t * t + 2) + b;
                case Type.EaseInQuart:
                    return c * t * t * t * t + b;
                case Type.EaseOutQuart:
                    t -= 1;
                    return -c * (t * t * t * t - 1) + b;
                case Type.EaseInOutQuart:
                    if ((t / 2) < 1) {
                        return c / 2 * t * t * t * t + b;
                    }

                    t -= 2;
                    return -c / 2 * (t * t * t * t - 2) + b;
                case Type.EaseInQuint:
                    return c * t * t * t * t * t + b;
                case Type.EaseOutQuint:
                    t -= 1;
                    return c * (t * t * t * t * t + 1) + b;
                case Type.EaseInOutQuint:
                    if ((t / 2) < 1) {
                        return c / 2 * t * t * t * t * t + b;
                    }

                    t -= 2;
                    return c / 2 * (t * t * t * t * t + 2) + b;
                case Type.EaseInSine:
                    return -c * Mathf.Cos(t * (Mathf.PI / 2)) + c + b;
                case Type.EaseOutSine:
                    return c * Mathf.Sin(t * (Mathf.PI / 2)) + b;
                case Type.EaseInOutSine:
                    return -c / 2 * (Mathf.Cos(Mathf.PI * t) - 1) + b;
                case Type.EaseInExpo:
                    return (t == 0) ? b : c * Mathf.Pow(2, 10 * (t - 1)) + b;
                case Type.EaseOutExpo:
                    return (t == 1) ? b + c : c * (-Mathf.Pow(2, -10 * t) + 1) + b;
                case Type.EaseInOutExpo:
                    if (t == 0) {
                        return b;
                    }

                    if (t == 1) {
                        return b + c;
                    }

                    if ((t / 2) < 1) {
                        return c / 2 * Mathf.Pow(2, 10 * (t - 1)) + b;
                    }

                    return c / 2 * (-Mathf.Pow(2, -10 * --t) + 2) + b;
                case Type.EaseInCirc:
                    return -c * (Mathf.Sqrt(1 - (t * t) - 1)) + b;
                case Type.EaseOutCirc:
                    t -= 1;
                    return c * Mathf.Sqrt(1 - t * t) + b;
                case Type.EaseInOutCirc:
                    if ((t / 2) < 1) {
                        return -c / 2 * (Mathf.Sqrt(1 - t * t) - 1) + b;
                    }

                    t -= 2;
                    return c / 2 * (Mathf.Sqrt(1 - t * t) + 1) + b;
                case Type.EaseInElastic:
                    return elasticEaseIn(b, c, t);
                case Type.EaseOutElastic:
                    return elasticEaseOut(b, c, t);
                case Type.EaseInBack:
                    return easeInBack(b, c, t);
                case Type.EaseOutBack:
                    return easeOutBack(b, c, t);
            }

            return 0;
        }

        private float elasticEaseIn(float b, float c, float t) {
            var ts = t * t;
            var tc = ts * t;
            return b + c * (33 * tc * ts + -59 * ts * ts + 32 * tc + -5 * ts);
        }

        private float elasticEaseOut(float b, float c, float t) {
            var ts = t * t;
            var tc = ts * t;
            return b + c * (33 * tc * ts + -106 * ts * ts + 126 * tc + -67 * ts + 15 * t);
        }

        private float easeInBack(float b, float c, float t) {
            var ts = t * t;
            var tc = ts * t;
            return b + c * (4 * tc + -3 * ts);
        }

        private float easeOutBack(float b, float c, float t) {
            var ts = t * t;
            var tc = ts * t;
            return b + c * (4 * tc + -9 * ts + 6 * t);
        }
    }
}