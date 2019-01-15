using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimationController {

        private int _currentFrameIndex = 0;
        private bool _playing = false;
        private UnscaledTimer _frameTimer = new UnscaledTimer();
        private SpriteAnimation _animation = null;

        private AnimationFrame CurrentFrame { get { return _animation != null ? _animation.GetFrame(_currentFrameIndex) : null; } }
        public int FrameIndex { get { return _currentFrameIndex; } }
        public bool Active { get { return _animation != null && _playing; } }
        public Sprite GetFrameSprite { get { return CurrentFrame != null ? _animation.GetSpriteFrame(_currentFrameIndex) : null; } }

        public void ResetAnimation(SpriteAnimation spriteAnimation) {
            _currentFrameIndex = 0;
            _animation = spriteAnimation;
            if (_animation == null) {
                _playing = false;
                return;
            }
            _playing = true;
            _frameTimer.StartTimer();
        }

        public bool CheckFrameUpdate() {
            if (!_playing || _frameTimer.IsActive) {
                return false;
            }
            _currentFrameIndex++;
            if (CurrentFrame != null) {
                _frameTimer.StartNewTime(_animation.FrameTime * CurrentFrame.Length);
                return true;
            }
            if (!_animation.IsComplete(_currentFrameIndex)) {
                return true;
            }
            if (_animation.Looping) {
                _currentFrameIndex = 0;
                _frameTimer.StartNewTime(_animation.FrameTime * CurrentFrame.Length);
                return true;
            }
            _playing = false;
            return false;
        }
    }
}
