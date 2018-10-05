using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    
    public class Tween {
        public enum TweenRepeat {
            Once,
            PingPong,
            Loop
        }

        public enum TweenType {
            delay,
            f,
            v2,
            v3,
            c
        }

        public const Action doNothing = null;
        private Action<float> valSet;
        private Action<Color> valSetC;
        private Action<Vector2> valSetv2;
        private Action<Vector3> valSetv3;
        public Func<float, float, float, float> easeFunc;
        public float from, to, resttime, originaltime, progress;
        public Color fromC, toC;
        public Vector2 fromV2, toV2;
        public Vector3 fromV3, toV3;
        public Action OnComplete;
        public TweenRepeat repeat = TweenRepeat.Once;
        public Func<float> time;
        public TweenType type = TweenType.f;

        /// <summary>
        ///     Use this for animating float values. The first parameter has to a setter for the float
        /// </summary>
        /// <param name="valueSetter">Value setter.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="length">Length.</param>
        /// <param name="easeType">Ease type.</param>
        /// <param name="unscaled">If set to <c>true</c> unscaled.</param>
        /// <param name="repeat">Repeat.</param>
        /// <param name="OnComplete">On complete.</param>
        public Tween(Action<float> valueSetter, float from, float to, float length,
            EasingTypes easeType = EasingTypes.Linear, bool unscaled = false, TweenRepeat repeat = TweenRepeat.Once,
            Action OnComplete = doNothing) {
            valSet = valueSetter;
            this.from = from;
            this.to = to;
            easeFunc = Easing.Function(easeType);
            resttime = originaltime = length;
            this.OnComplete = OnComplete;
            type = TweenType.f;
            this.repeat = repeat;
            time = timeFunc(unscaled);
        }

        /// <summary>
        ///     Use this for animating a Vector2
        /// </summary>
        /// <param name="valueSetter">Value setter.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="length">Length.</param>
        /// <param name="easeType">Ease type.</param>
        /// <param name="unscaled">If set to <c>true</c> unscaled.</param>
        /// <param name="repeat">Repeat.</param>
        /// <param name="OnComplete">On complete.</param>
        public Tween(Action<Vector2> valueSetter, Vector2 from, Vector2 to, float length,
            EasingTypes easeType = EasingTypes.Linear, bool unscaled = false, TweenRepeat repeat = TweenRepeat.Once,
            Action OnComplete = doNothing) {
            valSetv2 = valueSetter;
            fromV2 = from;
            toV2 = to;
            easeFunc = Easing.Function(easeType);
            resttime = originaltime = length;
            this.OnComplete = OnComplete;
            type = TweenType.v2;
            this.repeat = repeat;
            time = timeFunc(unscaled);
        }

        /// <summary>
        ///     Use this for animating a Vector3
        /// </summary>
        /// <param name="valueSetter">Value setter.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="length">Length.</param>
        /// <param name="easeType">Ease type.</param>
        /// <param name="unscaled">If set to <c>true</c> unscaled.</param>
        /// <param name="repeat">Repeat.</param>
        /// <param name="OnComplete">On complete.</param>
        public Tween(Action<Vector3> valueSetter, Vector3 from, Vector3 to, float length,
            EasingTypes easeType = EasingTypes.Linear, bool unscaled = false, TweenRepeat repeat = TweenRepeat.Once,
            Action OnComplete = doNothing) {
            valSetv3 = valueSetter;
            fromV3 = from;
            toV3 = to;
            easeFunc = Easing.Function(easeType);
            resttime = originaltime = length;
            this.OnComplete = OnComplete;
            type = TweenType.v3;
            this.repeat = repeat;
            time = timeFunc(unscaled);
        }

        /// <summary>
        ///     Use this for animating colours
        /// </summary>
        /// <param name="valueSetter">Value setter.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="length">Length.</param>
        /// <param name="easeType">Ease type.</param>
        /// <param name="unscaled">If set to <c>true</c> unscaled.</param>
        /// <param name="repeat">Repeat.</param>
        /// <param name="OnComplete">On complete.</param>
        public Tween(Action<Color> valueSetter, Color from, Color to, float length,
            EasingTypes easeType = EasingTypes.Linear, bool unscaled = false, TweenRepeat repeat = TweenRepeat.Once,
            Action OnComplete = doNothing) {
            valSetC = valueSetter;
            fromC = from;
            toC = to;
            easeFunc = Easing.Function(easeType);
            resttime = originaltime = length;
            this.OnComplete = OnComplete;
            type = TweenType.c;
            this.repeat = repeat;
            time = timeFunc(unscaled);
        }

        public Tween(float seconds) {
            originaltime = seconds;
            type = TweenType.delay;
        }

        public Vector2 ValueV2 { set { valSetv2(value); } }

        public Vector3 ValueV3 { set { valSetv3(value); } }

        public Color ValueC { set { valSetC(value); } }

        public float Value { set { valSet(value); } }

        public static void swap<T>(ref T param1, ref T param2) {
            var tmp = param1;
            param1 = param2;
            param2 = tmp;
        }

        public void SwitchTargets() {
            swap(ref from, ref to);
            swap(ref fromV2, ref toV2);
            swap(ref fromV3, ref toV3);
            swap(ref fromC, ref toC);
        }

        public static Func<float> timeFunc(bool unscaled) {
            return unscaled ? (() => Time.unscaledDeltaTime) : (Func<float>) (() => Time.deltaTime);
        }

        public Task Play() {
            return TimeManager.StartUnscaled(Animate());
        }

        private IEnumerator Animate() {
            //store the easeCall in this Action depending on Tweentype
            Action easeCall;
            switch (type) {
                case Tween.TweenType.delay:
                    yield return originaltime;
                    yield break;
                case Tween.TweenType.f:
                    easeCall = () => { Value = easeFunc(from, to, progress); };
                    break;
                case Tween.TweenType.v2:
                    easeCall = () => { ValueV2 = Vector2.zero.ease(easeFunc, fromV2, toV2, progress); };
                    break;
                case Tween.TweenType.v3:
                    easeCall = () => { ValueV3 = Vector3.zero.ease(easeFunc, fromV3, toV3, progress); };
                    break;
                case Tween.TweenType.c:
                    easeCall = () => { ValueC = Color.black.ease(easeFunc, fromC, toC, progress); };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //the actual easing over time
            while (resttime > 0) {
                resttime -= time();
                progress = 1f - resttime/originaltime;
                easeCall();
                yield return null;
            }
            if (OnComplete != null) {
                OnComplete();
            }
            CheckRepeat();
        }

        private void CheckRepeat() {
            resttime = originaltime;
            if (repeat == Tween.TweenRepeat.PingPong) { SwitchTargets(); }
            if (repeat == Tween.TweenRepeat.PingPong || repeat == Tween.TweenRepeat.Loop) {
                TimeManager.StartTask(Animate());
            }
        }
    }
}