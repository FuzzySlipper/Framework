using UnityEngine;
using System.Collections;
using System;

namespace PixelComrades {
    public abstract class Tweener {

        [SerializeField] private EasingTypes _easeType = EasingTypes.Linear;
        [SerializeField] private float _length = 1;
        [SerializeField] private bool _unScaled = true;

        protected bool Initialized = false;

        private Func<float, float, float, float> _easeFunc;
        private float _start;
        private float _percent = 0;

        protected float Time { get { return _unScaled ? TimeManager.TimeUnscaled : TimeManager.Time; } }
        public virtual bool Active { get { return _percent < 1 && Initialized; } }
        public float Percent { get { return _percent; } }
        public float Length { get { return _length; } set { _length = value; } }
        public EasingTypes EasingConfig { get { return _easeType; } set { _easeType = value; } }
        public bool UnScaled { get { return _unScaled; } set { _unScaled = value; } }

        public void Init() {
            _start = Time;
            _easeFunc = Easing.Function(_easeType);
            Initialized = true;
            _percent = 0;
            if (_length <= 0) {
                _length = 0.1f;
            }
        }

        protected float GetEase(float start, float end) {
            if (!Initialized) {
                Init();
            }
            _percent = (Time - _start) / _length;
            return _easeFunc(start, end, _percent);
        }

        public void Cancel() {
            Initialized = false;
        }

        protected IEnumerator Animate(System.Action performTween, System.Action onComplete) {
            Init();
            while (Active) {
                performTween();
                yield return null;
            }
            if (onComplete != null) {
                onComplete();
            }
        }

        public virtual void Flip() { }
    }

    [System.Serializable] public class TweenV3 : Tweener {

        [SerializeField] private Vector3 _startValue = Vector3.zero;
        [SerializeField] private Vector3 _endValue = Vector3.zero;

        public Task Play(System.Action<Vector3> set, System.Action onComplete) {
            return TimeManager.StartUnscaled(Animate(() => { set(Get()); }, onComplete));
        }

        public Vector3 EndValue { get { return _endValue; } }

        public TweenV3() {}

        public TweenV3(Vector3 start, Vector3 end, float duration, EasingTypes ease, bool isUnScaled = true) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            UnScaled = isUnScaled;
        }

        public void Restart(Vector3 start, Vector3 end, float duration, EasingTypes ease, bool isUnScaled = true) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            UnScaled = isUnScaled;
            Init();
        }

        public void Restart(Vector3 start, Vector3 end, float duration) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            Init();
        }

        public void Restart(Vector3 start, Vector3 end) {
            _startValue = start;
            _endValue = end;
            Init();
        }

        public override void Flip() {
            var start = _endValue;
            var end = _startValue;
            _startValue = start;
            _endValue = end;
            Init();
        }

        public Vector3 Get() {
            return new Vector3(
                GetEase(_startValue.x, _endValue.x),
                GetEase(_startValue.y, _endValue.y),
                GetEase(_startValue.z, _endValue.z)
            );
        }
    }

    [System.Serializable] 
    public class TweenFloat : Tweener {
        private float _startValue;
        private float _endValue;

        public TweenFloat() {}

        public Task Play(System.Action<float> set, System.Action onComplete) {
            return TimeManager.StartUnscaled(Animate(() => { set(Get()); }, onComplete));
        }

        public TweenFloat(float start, float end, float duration, EasingTypes ease, bool unScaled) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            UnScaled = unScaled;
        }

        public void Restart(float start, float end, float duration, EasingTypes ease, bool unScaled) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            UnScaled = unScaled;
            Init();
        }

        public void Restart(float start, float end, float duration) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            Init();
        }

        public void Restart(float start, float end) {
            _startValue = start;
            _endValue = end;
            Init();
        }

        public override void Flip() {
            var start = _endValue;
            var end = _startValue;
            _startValue = start;
            _endValue = end;
            Init();
        }

        public float Get() {
            return GetEase(_startValue, _endValue);
        }
    }

    [System.Serializable] public class TweenQuaternion : Tweener {

        [SerializeField] private Quaternion _startValue;
        [SerializeField] private Quaternion _endValue;

        public TweenQuaternion() {
        }

        public TweenQuaternion(Quaternion start, Quaternion end, float duration, EasingTypes ease, bool unscaled = true) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            UnScaled = unscaled;
            EasingConfig = ease;
        }

        public Task Play(System.Action<Quaternion> set, System.Action onComplete) {
            return TimeManager.StartUnscaled(Animate(() => { set(Get()); }, onComplete));
        }

        public void Restart(Quaternion start, Quaternion end, float duration, EasingTypes ease) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            Init();
        }

        public void Restart(Quaternion start, Quaternion end, float duration) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            Init();
        }

        public void Restart(Quaternion start, Quaternion end) {
            _startValue = start;
            _endValue = end;
            Init();
        }
        public override void Flip() {
            var start = _endValue;
            var end = _startValue;
            _startValue = start;
            _endValue = end;
            Init();
        }

        public Quaternion Get() {
            return Quaternion.Lerp(_startValue, _endValue, GetEase(0, 1));
        }
    }

    [System.Serializable] public class TweenColor : Tweener {
        private Color _startValue;
        private Color _endValue;

        public TweenColor() {
        }

        public TweenColor(Color start, Color end, float duration, EasingTypes ease, bool unScaled) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            UnScaled = unScaled;
        }

        public Task Play(System.Action<Color> set, System.Action onComplete) {
            return TimeManager.StartUnscaled(Animate(() => { set(Get()); }, onComplete));
        }

        public void Restart(Color start, Color end, float duration, EasingTypes ease, bool unScaled) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            EasingConfig = ease;
            UnScaled = unScaled;
            Init();
        }

        public void Restart(Color start, Color end, float duration) {
            _startValue = start;
            _endValue = end;
            Length = duration;
            Init();
        }

        public void Restart(Color start, Color end) {
            _startValue = start;
            _endValue = end;
            Init();
        }
        public override void Flip() {
            var start = _endValue;
            var end = _startValue;
            _startValue = start;
            _endValue = end;
            Init();
        }

        public Color Get() {
            return new Color(
                GetEase(_startValue.r, _endValue.r),
                GetEase(_startValue.g, _endValue.g),
                GetEase(_startValue.b, _endValue.b),
                GetEase(_startValue.a, _endValue.a)
            );
        }
    }
}
