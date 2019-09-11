using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class SpriteAnimator : MonoBehaviour, IOnCreate, IPoolEvents, ISystemUpdate {

        [SerializeField] private SpriteAnimation _animation = null;
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private bool _unscaled = true;
        [SerializeField] private bool _backwards = false;
        [SerializeField] private BillboardMode _billboard = BillboardMode.CamFwd;

        private SpriteAnimationController _spriteAnimator;
        private SpriteCollider _spriteCollider;
        private bool _active = true;
        private MaterialPropertyBlock _materialBlock;
        private float _lastAngleHeight;
        public SpriteRenderer Renderer { get => _renderer; }
        public SpriteAnimationController Animator { get => _spriteAnimator; }

        public void OnCreate(PrefabEntity entity) {
            _materialBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_materialBlock);
            _spriteAnimator = new SpriteAnimationController(_unscaled);
            _spriteCollider = _renderer.GetComponent<SpriteCollider>();
            if (_animation != null) {
                SetAnimation(_animation);
            }
        }

        public void OnPoolSpawned() {
            if (_animation != null) {
                _active = true;
                Play();
            }
        }

        public void OnPoolDespawned() {
            _active = false;
        }

        public void ClearAnimation() {
            _animation = null;
        }

        private void SetAnimation(SpriteAnimation anim) {
            _animation = anim;
            _renderer.sprite = _animation.GetSpriteFrame(0);
            if (_animation.NormalMap != null) {
                _materialBlock.SetTexture("_BumpMap", _animation.NormalMap);
            }
            _materialBlock.SetTexture("_MainTex", _renderer.sprite.texture);
            _renderer.SetPropertyBlock(_materialBlock);
        }

        public void Play(SpriteAnimation anim) {
            SetAnimation(anim);
            Play();
        }

        public void Play() {
            if (_spriteAnimator == null) {
                _spriteAnimator = new SpriteAnimationController(_unscaled);
            }
            _spriteAnimator.ResetAnimation(_animation);
            UpdateSpriteFrame();
        }

        void OnBecameVisible() {
            _active = true;
        }

        void OnBecameInvisible() {
            _active = false;
        }

        public bool Unscaled { get { return _unscaled; } }

        public void OnSystemUpdate(float dt) {
            if (!_active || _animation == null) {
                return;
            }
            _billboard.Apply(_renderer.transform, _backwards, ref _lastAngleHeight);
            if (_spriteAnimator.CheckFrameUpdate()) {
                UpdateSpriteFrame();
            }
        }
        
        private void UpdateSpriteFrame() {
            var sprite = _animation.GetSpriteFrame(_spriteAnimator.FrameIndex);
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

        private IEnumerator TestAnimationRunner() {
            _spriteAnimator.ResetAnimation(_animation);
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
                _billboard.Apply(_renderer.transform, _backwards, ref _lastAngleHeight);
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
