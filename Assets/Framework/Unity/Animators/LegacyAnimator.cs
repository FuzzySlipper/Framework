using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class LegacyAnimator : TargetAnimator {

        [SerializeField] private Animation _animation = null;
        [SerializeField] private AnimationClip[] _animationClips = new AnimationClip[0];
        [SerializeField] private bool _reversePlay = false;

        private int _animationIndex = 0;
        private bool _reversed = false;

        [ContextMenu("Play")]
        public override void Play() {
            if (_reversePlay) {
                _animation[_animationClips[_animationIndex].name].speed = !_reversed ? 1 : -1;
                _reversed = !_reversed;
            }
            _animation.Play(_animationClips[_animationIndex].name);
            _animationIndex++;
            if (_animationIndex >= _animationClips.Length) {
                _animationIndex = 0;
            }
            
        }

        public override float Length { get { return _animationIndex < _animationClips.Length ? _animationClips[_animationIndex].length : 0.5f; } }
        public override bool IsPlaying { get { return _animation.isPlaying; } }
    }
}
