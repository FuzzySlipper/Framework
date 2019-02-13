using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class DirectionalSpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents {

        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;
        [SerializeField] private bool _backwards = false;
        [SerializeField] private SpriteFacing _facing = SpriteFacing.Fourway;
        [SerializeField] private DirectionalAnimation _animation = null;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private bool _unscaled = true;

        private DirectionsEight _orientation = DirectionsEight.Top;
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
            var orientation = SpriteFacingControl.GetCameraSide(_facing, transform, transform.parent != null ? transform.parent : transform, 5, out var inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                return;
            }
            _orientation = orientation;
            UpdateSpriteFrame();
        }

        private void UpdateSpriteFrame() {
            var facing = _orientation;
            if (_facing.RequiresFlipping()) {
                facing = _orientation.GetFlippedSide();
                _renderer.flipX = _orientation.IsFlipped();
            }
            var sprite = _animation.GetSpriteFrame(facing, _spriteAnimator.FrameIndex);
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

        [Button]
        public void TestLoopAnimation() {
            _spriteAnimator = new SpriteAnimationController(true);
            _looping = true;
            TimeManager.StartUnscaled(TestAnimationRunner());
        }

        [Button]
        public void StopLoop() {
            _looping = false;
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
                var orientation = SpriteFacingControl.GetCameraSide(_facing, transform, transform, 5, out var inMargin);
                _billboard.Apply(transform, _backwards);
                if (_spriteAnimator.CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                else if (_orientation != orientation && (!inMargin || (!orientation.IsAdjacent(_orientation)))) {
                    _orientation = orientation;
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


