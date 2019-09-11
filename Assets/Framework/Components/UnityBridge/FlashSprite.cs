using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteColorComponent : ComponentBase, IReceive<DamageEvent>, IReceive<StunEvent>, IReceive<SlowEvent>, IReceive<ConfusionEvent> {

        private const float Duration = 0.5f;

        private string _shaderColor;
        private SpriteRenderer _spriteRender;
        private float _dmgMaxScale;
        private TweenFloat _scaleDmgTween;
        private MaterialPropertyBlock _matBlock;
        private Color _baseColor = Color.white;
        private bool _animatingColor = false;

        private static GameOptions.CachedColor _stunColor = new GameOptions.CachedColor("Stunned");
        private static GameOptions.CachedColor _confusedColor = new GameOptions.CachedColor("Confused");
        private static GameOptions.CachedColor _frozenColor = new GameOptions.CachedColor("Frozen");

        public SpriteColorComponent(SpriteRenderer renderer, string shaderColor = "_Color", float maxDamageScale = 1.15f) {
            _spriteRender = renderer;
            _shaderColor = shaderColor;
            _dmgMaxScale = maxDamageScale;
            _scaleDmgTween = new TweenFloat(0, 1, Duration, EasingTypes.BounceInOut, false);
            _matBlock = new MaterialPropertyBlock();
            _spriteRender.GetPropertyBlock(_matBlock);
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
            _spriteRender.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, Color.red);
            _spriteRender.SetPropertyBlock(_matBlock);
            _scaleDmgTween.Restart(1, _dmgMaxScale);
            var scale = _spriteRender.transform.localScale;
            while (_scaleDmgTween.Active) {
                _spriteRender.transform.localScale = new Vector3(scale.x * _scaleDmgTween.Get(),
                    scale.y, scale.z);
                yield return null;
            }
            _scaleDmgTween.Restart(_dmgMaxScale,1);
            while (_scaleDmgTween.Active) {
                _spriteRender.GetPropertyBlock(_matBlock);
                _spriteRender.transform.localScale = new Vector3(scale.x * _scaleDmgTween.Get(),
                    scale.y, scale.z);
                _matBlock.SetColor(_shaderColor, Color.Lerp(Color.red, _baseColor, _scaleDmgTween.Get()));
                _spriteRender.SetPropertyBlock(_matBlock);
                yield return null;
            }
            _animatingColor = false;
            _spriteRender.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, _baseColor);
            _spriteRender.SetPropertyBlock(_matBlock);
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
            if (Entity.Tags.IsStunned || tagType == TagTypes.Stun) {
                _baseColor = _stunColor;
            }
            else if (Entity.Tags.IsConfused || tagType == TagTypes.Confuse) {
                _baseColor = _confusedColor;
            }
            else if (Entity.Tags.IsSlowed || tagType == TagTypes.Slow) {
                _baseColor = _frozenColor;
            }
            else {
                _baseColor = Color.white;
            }
            if (_animatingColor) {
                return;
            }
            _spriteRender.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, _baseColor);
            _spriteRender.SetPropertyBlock(_matBlock);
        }

        enum TagTypes {
            None,
            Stun,
            Slow,
            Confuse
        }
    }
}
