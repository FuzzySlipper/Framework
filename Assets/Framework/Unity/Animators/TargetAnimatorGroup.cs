using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class TargetAnimatorGroup : TargetAnimator {

        [SerializeField] private TargetAnimator[] _animators = new TargetAnimator[2];

        public override bool IsPlaying { get { return _playing != null; } }
        public override float Length {
            get {
                float cnt = 0;
                for (int i = 0; i < _animators.Length; i++) {
                    cnt += _animators[i].Length;
                }
                return cnt;
            }
        }

        private Task _playing = null;

        [Button("Test")]
        public void Test() {
            for (int i = 0; i < _animators.Length; i++) {
                var animator = _animators[i] as TweenAnimator;
                if (animator == null) {
                    var group = _animators[i] as TargetAnimatorGroup;
                    if (group != null) {
                        group.Test();
                    }
                    continue;
                }
                TimeManager.StartUnscaled(animator.PlayAnimation()); 
            }
        }

        public void Play(System.Action onFinish) {
            for (int i = 0; i < _animators.Length; i++) {
                _animators[i].Play();
            }
            _playing = TimeManager.StartUnscaled(WaitForTween(onFinish));
        }

        public override void PlayFrame(float normalized) {
            for (int i = 0; i < _animators.Length; i++) {
                _animators[i].PlayFrame(normalized);
            }
        }

        private IEnumerator WaitForTween(System.Action del) {
            yield return null;
            bool animationsPlaying = true;
            int cnt = 0;
            while (animationsPlaying) {
                animationsPlaying = false;
                for (int i = 0; i < _animators.Length; i++) {
                    if (_animators[i].IsPlaying) {
                        animationsPlaying = true;
                        break;
                    }
                }
                cnt++;
                yield return null;
                if (cnt > 15500) {
                    break;
                }
            }
            _playing = null;
            if (del != null) {
                del();
            }
        }

        public override void Play() {
            Play(null);
        }

        
    }
}
