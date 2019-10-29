using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SystemsTest : MonoBehaviour {

        [SerializeField] private SpriteAnimation _animation = null;
        [SerializeField] private bool _unscaled = true;
        [SerializeField] private Transform _drawMeshTr = null;

        private SpriteRenderer[] _sprites;

        void Awake() {
            var tr = _drawMeshTr != null ? _drawMeshTr : transform;
            _sprites = tr.GetComponentsInChildren<SpriteRenderer>();
        }

        void OnEnable() {
            if (_sprites == null) {
                var tr = _drawMeshTr != null ? _drawMeshTr : transform;
                _sprites = tr.GetComponentsInChildren<SpriteRenderer>();
            }
            var animationSystem = World.Get<FrameAnimationSystem>();
            for (int i = 0; i < _sprites.Length; i++) {
                if (_sprites[i] == null) {
                    continue;
                }
                //if (_drawMeshTr != null) {
                //    animationSystem.AddAnimation1(_animation, _sprites[i], _unscaled);
                //}
                //else {
                    animationSystem.AddAnimation(_animation, _sprites[i], _unscaled);
                //}
            }
        }
    }

    public class FrameAnimationSystem : SystemBase, IMainSystemUpdate {

        private GenericPool<AnimationPlayer> _pool = new GenericPool<AnimationPlayer>(100, player => player.Clear());
        private List<AnimationPlayer> _current = new List<AnimationPlayer>();

        //private GenericPool<SpriteRendererPlayer> _pool1 = new GenericPool<SpriteRendererPlayer>(100, player => player.Clear());
        //private List<SpriteRendererPlayer> _current1 = new List<SpriteRendererPlayer>();

        private class AnimationPlayer {

            private SpriteRenderer _renderer = null;
            private bool _unscaled = true;
            private SpriteAnimation _animation = null;

            private int _currentFrameIndex = 0;
            private bool _playing = false;
            private bool _activated = false;
            private float _animationStarted = 0;
            private float _nextUpdateTime;
            //private MaterialPropertyBlock _materialBlock = new MaterialPropertyBlock();

            private AnimationFrame CurrentFrame { get { return _animation != null ? _animation.GetFrame(_currentFrameIndex) : null; } }
            private int FrameIndex { get { return _currentFrameIndex; } }
            public bool Finished { get { return !_playing; } }

            public void Setup(SpriteRenderer renderer, SpriteAnimation animation, bool unscaled, float time) {
                _renderer = renderer;
                _animation = animation;
                _unscaled = unscaled;
                //_renderer.GetPropertyBlock(_materialBlock);
                _renderer.sprite = _animation.GetSprite(0);
                //if (_animation.NormalMap != null) {
                //    _materialBlock.SetTexture("_BumpMap", _animation.NormalMap);
                //}
                //_materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
                //_renderer.SetPropertyBlock(_materialBlock);
                _currentFrameIndex = 0;
                _playing = true;
                _activated = true;
                StartNewTime(_animation.FrameTime * CurrentFrame.Length, time);
                _animationStarted = time;
            }

            public void Clear() {
                _renderer = null;
                _animation = null;
            }

            public void UpdateSpriteFrame(float time) {
                if (!_playing || !_activated || time < _nextUpdateTime) {
                    return;
                }
                _currentFrameIndex++;
                var cf = CurrentFrame;
                if (cf != null) {
                    StartNewTime(_animation.FrameTime * cf.Length, time);
                    _renderer.sprite = _animation.GetSprite(FrameIndex);
                    return;
                }
                if (!_animation.IsComplete(_currentFrameIndex)) {
                    return;
                }
                if (_animation.Looping) {
                    _currentFrameIndex = 0;
                    StartNewTime(_animation.FrameTime * cf.Length, time);
                    _renderer.sprite = _animation.GetSprite(FrameIndex);
                    return;
                }
                _playing = false;
            }

            private void StartNewTime(float length, float time) {
                _nextUpdateTime = time + length;
                _activated = true;
            }
        }

        public void AddAnimation(SpriteAnimation animation, SpriteRenderer renderer, bool unscaled) {
            var node = _pool.New();
            node.Setup(renderer, animation, unscaled, TimeManager.Time);
            _current.Add(node);
        }

        //public void AddAnimation1(SpriteAnimation animation, SpriteRenderer renderer, bool unscaled) {
        //    var node = _pool1.New();
        //    node.Setup(renderer, animation, unscaled);
        //    _current1.Add(node);
        //}

        public void OnSystemUpdate(float dt, float unscaledDt) {
            var time = TimeManager.TimeUnscaled;
            for (int i = _current.Count - 1; i >= 0; i--) {
                _current[i].UpdateSpriteFrame(time);
                if (_current[i].Finished) {
                    _pool.Store(_current[i]);
                    _current.RemoveAt(i);
                }
            }
            //for (int i = _current1.Count - 1; i >= 0; i--) {
            //    _current1[i].UpdateMesh();
            //    if (_current1[i].Finished) {
            //        _pool1.Store(_current1[i]);
            //        _current1.RemoveAt(i);
            //    }
            //}
        }
    }
}
