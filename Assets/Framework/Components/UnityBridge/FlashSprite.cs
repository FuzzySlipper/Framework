using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpriteColorComponent : IComponent, IReceive<DamageEvent>, IReceive<StunEvent>, IReceive<SlowEvent>, IReceive<ConfusionEvent> {

        private const float Duration = 0.5f;

        private string _shaderColor;
        private CachedUnityComponent<SpriteRenderer> _spriteRender;
        private float _dmgMaxScale;
        private Color _baseColor = Color.white;
        
        
        private TweenFloat _scaleDmgTween;
        private MaterialPropertyBlock _matBlock;
        private bool _animatingColor = false;
        
        private SpriteRenderer Renderer { get { return _spriteRender.Value; } }
        
        private static GameOptions.CachedColor _stunColor = new GameOptions.CachedColor("Stunned");
        private static GameOptions.CachedColor _confusedColor = new GameOptions.CachedColor("Confused");
        private static GameOptions.CachedColor _frozenColor = new GameOptions.CachedColor("Frozen");

        public SpriteColorComponent(SpriteRenderer renderer, string shaderColor = "_Color", float maxDamageScale = 1.15f) {
            _spriteRender = new CachedUnityComponent<SpriteRenderer>(renderer);
            _shaderColor = shaderColor;
            _dmgMaxScale = maxDamageScale;
            Setup();
        }

        public SpriteColorComponent(SerializationInfo info, StreamingContext context) {
            _shaderColor = info.GetValue(nameof(_shaderColor), _shaderColor);
            _spriteRender = info.GetValue(nameof(_spriteRender), _spriteRender);
            _dmgMaxScale = info.GetValue(nameof(_dmgMaxScale), _dmgMaxScale);
            _baseColor = info.GetValue(nameof(_baseColor), _baseColor);
        }

        private void Setup() {
            _scaleDmgTween = new TweenFloat(0, 1, Duration, EasingTypes.BounceInOut, false);
            _matBlock = new MaterialPropertyBlock();
            _spriteRender.Value.GetPropertyBlock(_matBlock);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_shaderColor), _shaderColor);
            info.AddValue(nameof(_spriteRender), _spriteRender);
            info.AddValue(nameof(_dmgMaxScale), _dmgMaxScale);
            info.AddValue(nameof(_baseColor), _baseColor);
            Setup();
        }

        public void ChangeBaseColor(Color color) {
            _baseColor = color;
        }

        public void Handle(DamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            if (!_animatingColor && _spriteRender != null) {
                TimeManager.StartTask(DamageTween());
            }
            else if (_animatingColor && _spriteRender != null) {
                _scaleDmgTween.Restart(_dmgMaxScale, 1);
            }
        }
        
        private IEnumerator DamageTween() {
            _animatingColor = true;
            Renderer.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, Color.red);
            Renderer.SetPropertyBlock(_matBlock);
            _scaleDmgTween.Restart(1, _dmgMaxScale);
            var scale = Renderer.transform.localScale;
            while (_scaleDmgTween.Active) {
                Renderer.transform.localScale = new Vector3(scale.x * _scaleDmgTween.Get(),
                    scale.y, scale.z);
                yield return null;
            }
            _scaleDmgTween.Restart(_dmgMaxScale,1);
            while (_scaleDmgTween.Active) {
                Renderer.GetPropertyBlock(_matBlock);
                Renderer.transform.localScale = new Vector3(scale.x * _scaleDmgTween.Get(),
                    scale.y, scale.z);
                _matBlock.SetColor(_shaderColor, Color.Lerp(Color.red, _baseColor, _scaleDmgTween.Get()));
                Renderer.SetPropertyBlock(_matBlock);
                yield return null;
            }
            _animatingColor = false;
            Renderer.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, _baseColor);
            Renderer.SetPropertyBlock(_matBlock);
        }

        public void Handle(StunEvent arg) {
            CheckTagColor(arg.Active? TagTypes.Stun : TagTypes.None);
        }

        public void Handle(SlowEvent arg) {
            CheckTagColor(arg.Active ? TagTypes.Slow : TagTypes.None);
        }

        public void Handle(ConfusionEvent arg) {
            CheckTagColor(arg.Active ? TagTypes.Confuse : TagTypes.None);
        }

        private void CheckTagColor(TagTypes tagType) {
            var entity = this.GetEntity();
            if (entity.Tags.IsStunned || tagType == TagTypes.Stun) {
                _baseColor = _stunColor;
            }
            else if (entity.Tags.IsConfused || tagType == TagTypes.Confuse) {
                _baseColor = _confusedColor;
            }
            else if (entity.Tags.IsSlowed || tagType == TagTypes.Slow) {
                _baseColor = _frozenColor;
            }
            else {
                _baseColor = Color.white;
            }
            if (_animatingColor) {
                return;
            }
            Renderer.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, _baseColor);
            Renderer.SetPropertyBlock(_matBlock);
        }

        enum TagTypes {
            None,
            Stun,
            Slow,
            Confuse
        }
    }
}
