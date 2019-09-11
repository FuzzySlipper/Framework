using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class DirectionalSpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents, ISystemUpdate {

        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;
        [SerializeField] private bool _backwards = false;
        [SerializeField] private SpriteFacing _facing = SpriteFacing.Fourway;
        [SerializeField] private DirectionalAnimation _sprite = null;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private bool _unscaled = true;

        private DirectionsEight _orientation = DirectionsEight.Top;
        private SpriteAnimationController _spriteAnimator = new SpriteAnimationController();
        private bool _active = true;
        private SpriteCollider _spriteCollider;
        private float _lastAngleHeight;

        public void OnCreate(PrefabEntity entity) {
            MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialBlock);
            _spriteCollider = _renderer.GetComponent<SpriteCollider>();
            if (_sprite == null) {
                return;
            }
            _renderer.sprite = _sprite.GetSpriteFrame(0);
            materialBlock.SetTexture("_BumpMap", _sprite.NormalMap);
            materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(materialBlock);
            if (_spriteCollider != null) {
                _spriteCollider.UpdateCollider();
            }
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

        public bool Unscaled { get { return _unscaled; } }

        public void OnSystemUpdate(float dt) {
            if (!_active) {
                return;
            }
            _billboard.Apply(_renderer.transform, _backwards, ref _lastAngleHeight);
            var orientation = SpriteFacingControl.GetCameraSide(_facing, transform, transform.parent, 5, out var inMargin);
            if (_orientation == orientation || (inMargin && (orientation.IsAdjacent(_orientation)))) {
                if (_spriteAnimator.CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
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
            var sprite = _sprite.GetSpriteFrame(facing, _spriteAnimator.FrameIndex);
            if (sprite == null) {
                return;
            }
            _renderer.sprite = sprite;
            if (_spriteCollider != null) {
                _spriteCollider.UpdateCollider();
            }
        }

#if UNITY_EDITOR
        private bool _looping;

        [SerializeField] private int _margin = 5;

        [Button]
        public void TestAnimation() {
            _spriteAnimator = new SpriteAnimationController(true);
            _looping = true;
            TimeManager.StartUnscaled(TestAnimationRunner());
        }

        [Button]
        public void StopLoop() {
            _looping = false;
        }

        [Button]
        private void TestBounds() {
            _spriteAnimator = new SpriteAnimationController(true);
            _looping = true;
            TimeManager.StartUnscaled(TestAnimationRunnerBounds());
        }

        private IEnumerator TestAnimationRunner() {
            _spriteAnimator.ResetAnimation(_sprite);
            UpdateSpriteFrame();
            int noCam = 0;
            while (_spriteAnimator.Active) {
                if (!_looping) {
                    break;
                }
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
                var facingTr = transform.parent != null ? transform.parent : transform;
                var orientation = SpriteFacingControl.GetCameraSide(_facing, facingTr, facingTr, _margin, out var inMargin);
                _billboard.Apply(_renderer.transform, _backwards, ref _lastAngleHeight);
                if (_spriteAnimator.CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                else if (_orientation != orientation && (!inMargin || (!orientation.IsAdjacent(_orientation)))) {
                    _orientation = orientation;
                    UpdateSpriteFrame();
                }
                if (!_spriteAnimator.Active && _looping) {
                    _spriteAnimator.ResetAnimation(_sprite);
                }
                yield return null;
            }
        }

        private IEnumerator TestAnimationRunnerBounds() {
            _spriteAnimator.ResetAnimation(_sprite);
            UpdateSpriteFrame();
            int noCam = 0;
            _spriteCollider = _renderer.GetComponent<SpriteCollider>();
            if (_spriteCollider != null) {
                _spriteCollider.OnCreate(null);
            }
            while (_spriteAnimator.Active) {
                if (!_looping) {
                    break;
                }
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
                _billboard.Apply(_renderer.transform, _backwards, ref _lastAngleHeight);
                if (_spriteAnimator.CheckFrameUpdate()) {
                    UpdateSpriteFrame();
                }
                else if (_orientation != orientation && (!inMargin || (!orientation.IsAdjacent(_orientation)))) {
                    _orientation = orientation;
                    UpdateSpriteFrame();
                }
                if (!_spriteAnimator.Active && _looping) {
                    _spriteAnimator.ResetAnimation(_sprite);
                }
                if (_spriteCollider != null) {
                    _spriteCollider.UpdateCollider();
                }
                yield return null;
            }
        }
#endif
    }
}