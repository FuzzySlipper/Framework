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

        public abstract Tweener Tween { get; }
        public abstract void StartTween();
        public abstract void UpdateTween();

        public override void Play() {
            if (_task != null) {
                TimeManager.Cancel(_task);
            }
            _task = TimeManager.StartTask(PlayAnimation(), UnScaled, Finish);
        }

        private void Finish() {
            _task = null;
            if (Repeat == PixelComrades.Tween.TweenRepeat.Loop) {
                Play();
            }
        }

        public IEnumerator PlayAnimation() {
            StartTween();
            bool flipped = false;
            while (true) {
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
            if (_chain != null) {
                yield return TimeManager.StartTask(_chain.PlayAnimation(), _chain.Tween.UnScaled);
            }
        }
    }
}
