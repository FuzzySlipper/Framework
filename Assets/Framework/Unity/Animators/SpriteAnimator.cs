using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class SpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents {

        [SerializeField] private SpriteAnimation _animation = null;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private bool _unscaled = true;
        [SerializeField] private bool _backwards = false;
        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;

        private SpriteAnimationController _spriteAnimator;
        private bool _active = true;

        public void OnCreate(PrefabEntity entity) {
            MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialBlock);
            _renderer.sprite = _animation.GetSpriteFrame(0);
            if (_animation.NormalMap != null) {
                materialBlock.SetTexture("_BumpMap", _animation.NormalMap);
            }
            materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(materialBlock);
            _spriteAnimator = new SpriteAnimationController(_unscaled);
        }

        public void Play() {
            if (_spriteAnimator == null) {
                _spriteAnimator = new SpriteAnimationController(_unscaled);
            }
            _spriteAnimator.ResetAnimation(_animation);
            UpdateSpriteFrame();
        }

        public void OnPoolSpawned() {
            Play();
        }

        public void OnPoolDespawned() { }

        void OnBecameVisible() {
            _active = true;
        }

        void OnBecameInvisible() {
            _active = false;
        }

        void Update() {
            if (!_active) {
                return;
            }
            _billboard.Apply(transform, _backwards);
            if (_spriteAnimator.CheckFrameUpdate()) {
                UpdateSpriteFrame();
            }
        }

        private void UpdateSpriteFrame() {
            var sprite = _animation.GetSpriteFrame(_spriteAnimator.FrameIndex);
            if (sprite != null) {
                _renderer.sprite = sprite;
            }
        }

#if UNITY_EDITOR
        [Button]
        public void TestAnimation() {
            _spriteAnimator = new SpriteAnimationController(true);
            TimeManager.StartUnscaled(TestAnimationRunner());
        }

        private bool _looping;
        //private Task _loop;
        [Button]
        public void TestLoopAnimation() {
            _spriteAnimator = new SpriteAnimationController(true);
            _looping = true;
            TimeManager.StartUnscaled(TestAnimationRunner());
            //_loop = TimeManager.StartUnscaled(TestAnimationRunner());
        }

        [Button]
        public void StopLoop() {
            _looping = false;
            //if (_loop != null) {
            //    TimeManager.Cancel(_loop);
            //    _loop = null;
            //}
        }

        private IEnumerator TestAnimationRunner() {
            _spriteAnimator.ResetAnimation(_animation);
            UpdateSpriteFrame();
            int noCam = 0;
            while (_spriteAnimator.Active) {
                if (Camera.current == null) {
                    yield return null;
                    noCam++;
                    if (noCam > 500) {
                        break;
                    }
                    continue;
                }
                Game.SpriteCamera = Camera.current;
                noCam = 0;
                _billboard.Apply(transform, _backwards);
                if (_spriteAnimator.CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                if (!_spriteAnimator.Active && _looping) {
                    _spriteAnimator.ResetAnimation(_animation);
                }
                yield return null;
            }
        }
#endif
    }
}
