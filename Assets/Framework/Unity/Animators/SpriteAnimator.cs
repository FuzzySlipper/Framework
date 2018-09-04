using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents {

        [SerializeField] private SpriteAnimation _animation = null;
        [SerializeField] private SpriteRenderer _renderer = null;

        private SpriteAnimationController _spriteAnimator = new SpriteAnimationController();
        private bool _active = true;

        public void OnCreate(PrefabEntity entity) {
            MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialBlock);
            _renderer.sprite = _animation.GetSpriteFrame(0);
            materialBlock.SetTexture("_BumpMap", _animation.NormalMap);
            materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(materialBlock);
        }

        public void OnPoolSpawned() {
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

        private void UpdateSpriteFrame() {
            var sprite = _animation.GetSpriteFrame(_spriteAnimator.FrameIndex);
            if (sprite != null) {
                _renderer.sprite = sprite;
            }
        }
    }
}
