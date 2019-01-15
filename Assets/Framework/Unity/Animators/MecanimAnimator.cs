using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MecanimAnimator : TargetAnimator {

        [SerializeField] private Animator _animator = null;
        [SerializeField] private string _trigger = "Trigger";

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
        }
    }
}
