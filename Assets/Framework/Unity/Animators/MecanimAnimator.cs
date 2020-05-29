using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MecanimAnimator : TargetAnimator, IPoolEvents {

        [SerializeField] private Animator _animator = null;
        [SerializeField] private string _trigger = "Trigger";

        private bool _triggered;
        private int _state;
        public override float Length {
            get {
                var clips = _animator.GetCurrentAnimatorClipInfo(0);
                if (clips != null) {
                    var clip = clips[0];
                    if (clip.clip != null) {
                        return clip.clip.length;
                    }
                }
                return 1;
            }
        }
        public override bool IsPlaying { get { return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1; }}

        public override void Play() {
            if (_animator != null) {
                _animator.SetTrigger(_trigger);
            }
            _triggered = true;
        }

        public override void PlayFrame(float normalized) {
            if (!_triggered) {
                if (_animator != null) {
                    _animator.SetTrigger(_trigger);
                }
                _triggered = true;
                TimeManager.StartUnscaled(PauseForTrigger(normalized));
                return;
            }
            if (_state < 0) {
                _state = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            }
            _animator.Play(_state, -1, normalized);
            _animator.speed = 0;
        }

        private IEnumerator PauseForTrigger(float normalized) {
            yield return null;
            PlayFrame(normalized);
        }

        public override void OnPoolSpawned() {
            base.OnPoolSpawned();
            _triggered = false;
        }

        public override void OnPoolDespawned() {
            base.OnPoolDespawned(); 
            
        }
    }
}
