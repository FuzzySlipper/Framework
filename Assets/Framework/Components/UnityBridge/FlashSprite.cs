using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpriteColorComponent : IComponent {
        
        public float DmgMaxScale;
        public Color BaseColor = Color.white;
        public bool AnimatingColor = false;
        private MaterialPropertyBlock _matBlock;
        private string _shaderColor;
        private CachedUnityComponent<SpriteRenderer> _spriteRender;
        public SpriteRenderer Renderer { get { return _spriteRender.Value; } }
        

        public SpriteColorComponent(SpriteRenderer renderer, string shaderColor = "_Color", float maxDamageScale = 1.15f) {
            _spriteRender = new CachedUnityComponent<SpriteRenderer>(renderer);
            _shaderColor = shaderColor;
            DmgMaxScale = maxDamageScale;
            Setup();
        }

        public SpriteColorComponent(SerializationInfo info, StreamingContext context) {
            _shaderColor = info.GetValue(nameof(_shaderColor), _shaderColor);
            _spriteRender = info.GetValue(nameof(_spriteRender), _spriteRender);
            DmgMaxScale = info.GetValue(nameof(DmgMaxScale), DmgMaxScale);
            BaseColor = info.GetValue(nameof(BaseColor), BaseColor);
            Setup();
        }

        private void Setup() {
            _matBlock = new MaterialPropertyBlock();
            _spriteRender.Value.GetPropertyBlock(_matBlock);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_shaderColor), _shaderColor);
            info.AddValue(nameof(_spriteRender), _spriteRender);
            info.AddValue(nameof(DmgMaxScale), DmgMaxScale);
            info.AddValue(nameof(BaseColor), BaseColor);
        }

        public void UpdateBaseColor() {
            Renderer.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, BaseColor);
            Renderer.SetPropertyBlock(_matBlock);
        }

        public void UpdateCurrentColor(Color color) {
            Renderer.GetPropertyBlock(_matBlock);
            _matBlock.SetColor(_shaderColor, color);
            Renderer.SetPropertyBlock(_matBlock);
        }
    }
}
