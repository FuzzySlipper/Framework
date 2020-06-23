using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimationController {

        public SpriteAnimationController(bool unscaled = true) {
            _frameTimer = new Timer(0, unscaled);
        }

        private int _currentFrameIndex = 0;
        private bool _playing = false;
        private Timer _frameTimer;
        
        private AnimationFrame CurrentFrame { get { return Animation != null ? Animation.GetFrame(_currentFrameIndex) : null; } }
        public SpriteAnimation Animation { get; private set; }
        public bool DefaultEventTriggered { get; private set; }
        public bool Finished { get; private set; }
        public AnimationFrame LastEventFrame { get; private set; }
        public int FrameIndex { get { return _currentFrameIndex; } }
        public bool Active { get { return Animation != null && _playing; } }
        public float TimeRemaining { get { return (Animation.LengthFrames - _currentFrameIndex) * Animation.FrameTime; } }
        public Sprite GetFrameSprite { get { return CurrentFrame != null ? Animation.GetSprite(_currentFrameIndex) : null; } }

        public void ResetAnimation(SpriteAnimation spriteAnimation) {
            _currentFrameIndex = 0;
            LastEventFrame = null;
            Animation = spriteAnimation;
            DefaultEventTriggered = Finished = false;
            if (Animation == null) {
                _playing = false;
                return;
            }
            _playing = true;
            //_frameTimer.StartTimer();
        }

        public void SkipFrame(int cnt) {
            for (int i = 0; i < cnt; i++) {
                _currentFrameIndex++;
                if (CurrentFrame != null) {
                    if (CurrentFrame.HasEvent) {
                        if (CurrentFrame.Event == AnimationFrame.EventType.Default) {
                            DefaultEventTriggered = true;
                        }
                        LastEventFrame = CurrentFrame;
                    }
                    _frameTimer.StartNewTime(Animation.FrameTime * CurrentFrame.Length);
                    continue;
                }
                if (!Animation.IsComplete(_currentFrameIndex)) {
                    continue;
                }
                if (Animation.Looping) {
                    _currentFrameIndex = 0;
                    _frameTimer.StartNewTime(Animation.FrameTime * CurrentFrame?.Length ?? 1);
                    break;
                }
                Finished = true;
                _playing = false;
                break;
            }
        }

        public bool CheckFrameUpdate() {
            if (!_playing || _frameTimer.IsActive || Animation == null) {
                return false;
            }
            _currentFrameIndex++;
            if (CurrentFrame != null) {
                if (CurrentFrame.HasEvent) {
                    if (CurrentFrame.Event == AnimationFrame.EventType.Default) {
                        DefaultEventTriggered = true;
                    }
                    LastEventFrame = CurrentFrame;
                }
                _frameTimer.StartNewTime(Animation.FrameTime * CurrentFrame.Length);
                return true;
            }
            if (!Animation.IsComplete(_currentFrameIndex)) {
                return true;
            }
            if (Animation.Looping) {
                _currentFrameIndex = 0;
                _frameTimer.StartNewTime(Animation.FrameTime * CurrentFrame?.Length ?? 1);
                return true;
            }
            Finished = true;
            _playing = false;
            return false;
        }
    }
}
