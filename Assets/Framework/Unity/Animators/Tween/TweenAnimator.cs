using UnityEngine;
using System.Collections;


namespace PixelComrades {
    public abstract class TweenAnimator : TargetAnimator {

        [SerializeField] private TweenAnimator _chain = null;
        [SerializeField] protected Transform Target = null;
        [SerializeField] protected bool UnScaled = false;
        [SerializeField] protected Tween.TweenRepeat Repeat = PixelComrades.Tween.TweenRepeat.Once;

        private Task _task;

        public TweenAnimator Chain {get { return _chain; } }
        public override float Length { get { return Tween.Length; } }
        public override bool IsPlaying { get { return Tween.Active; } }
        protected bool IsInvalid { get { return Target == null || gameObject == null || !gameObject.activeInHierarchy; } }

        public abstract Tweener Tween { get; }
        public abstract void StartTween();
        public abstract void UpdateTween();

        public override void Play() {
            if (_task != null) {
                TimeManager.Cancel(_task);
            }
            _task = TimeManager.StartTask(PlayAnimation(), UnScaled, Finish);
        }

        void OnDisable() {
            if (_task != null) {
                _task.Cancel();
                _task = null;
            }
        }

        public override void OnPoolDespawned() {
            if (_task != null) {
                _task.Cancel();
                _task = null;
            }
        }

        private void Finish() {
            _task = null;
            if (IsInvalid) {
                return;
            }
            if (Repeat == PixelComrades.Tween.TweenRepeat.Loop) {
                Play();
            }
        }

        public IEnumerator PlayAnimation() {
            StartTween();
            bool flipped = false;
            while (true) {
                if (IsInvalid) {
                    break;
                }
                UpdateTween();
                if (Tween.Active) {
                    yield return null;
                    continue;
                }
                if (Repeat == PixelComrades.Tween.TweenRepeat.PingPong) {
                    if (!flipped) {
                        flipped = true;
                        Tween.Flip();
                        continue;
                    }
                }
                break;
            }
            if (!IsInvalid && _chain != null) {
                yield return TimeManager.StartTask(_chain.PlayAnimation(), _chain.Tween.UnScaled);
            }
        }
    }
}
