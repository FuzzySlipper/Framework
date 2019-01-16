using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class SpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents {

        [SerializeField] private SpriteAnimation _animation = null;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private bool _unscaled = true;

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

        public void OnPoolSpawned() {
            if (_spriteAnimator == null) {
                _spriteAnimator = new SpriteAnimationController(_unscaled);
            }
            _spriteAnimator.ResetAnimation(_animation);
            UpdateSpriteFrame();
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
            if (_spriteAnimator.CheckFrameUpdate()) {
                UpdateSpriteFrame();
            }
        }

        [Button]
        public void TestAnimation() {
            _spriteAnimator = new SpriteAnimationController(true);
            TimeManager.StartUnscaled(TestAnimationRunner());
        }

        private IEnumerator TestAnimationRunner() {
            _spriteAnimator.ResetAnimation(_animation);
            UpdateSpriteFrame();
            while (_spriteAnimator.Active) {
                if (_spriteAnimator.CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                yield return null;
            }
        }

        private void UpdateSpriteFrame() {
            var sprite = _animation.GetSpriteFrame(_spriteAnimator.FrameIndex);
            if (sprite != null) {
                _renderer.sprite = sprite;
            }
        }
    }
}
