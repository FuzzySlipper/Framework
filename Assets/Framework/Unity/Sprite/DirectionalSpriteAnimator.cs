using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DirectionalSpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents {

        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;
        [SerializeField] private bool _backwards = false;
        [SerializeField] private SpriteFacing _facing = SpriteFacing.Fourway;
        [SerializeField] private DirectionalAnimation _sprite = null;
        [SerializeField] private SpriteRenderer _renderer = null;

        private DirectionsEight _orientation = DirectionsEight.Top;
        private SpriteAnimationController _spriteAnimator = new SpriteAnimationController();
        private bool _active = true;

        public void OnCreate(PrefabEntity entity) {
            MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialBlock);
            _renderer.sprite = _sprite.GetSpriteFrame(0);
            materialBlock.SetTexture("_BumpMap", _sprite.NormalMap);
            materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(materialBlock);
        }

        public void OnPoolSpawned() {
            _spriteAnimator.ResetAnimation(_sprite);
            UpdateSpriteFrame();
        }

        public void OnPoolDespawned() {}

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
            _billboard.Apply(transform, _backwards);
            bool inMargin;
            var orientation = SpriteFacingControl.GetCameraSide(_facing, transform, transform.parent, 5, out inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                return;
            }
            _orientation = orientation;
            UpdateSpriteFrame();
        }

        private void UpdateSpriteFrame() {
            var facing = _orientation;
            if (_facing == SpriteFacing.EightwayFlipped) {
                facing = _orientation.GetFlippedSide();
                _renderer.flipX = _orientation.IsFlipped();
            }
            var sprite = _sprite.GetSpriteFrame(facing, _spriteAnimator.FrameIndex);
            if (sprite != null) {
                _renderer.sprite = sprite;
            }
        }
    }
}


