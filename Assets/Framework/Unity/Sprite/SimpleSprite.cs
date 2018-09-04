using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class SimpleSprite : ScriptableObject {

        [SerializeField] private Sprite _sprite = null;
        [SerializeField] private Texture2D _normalMap = null;
        [SerializeField] private Texture2D _emissiveMap = null;

        public Sprite Sprite { get { return _sprite; } set { _sprite = value; } }
        public Texture2D NormalMap { get { return _normalMap; } set { _normalMap = value; } }
        public Texture2D EmissiveMap { get { return _emissiveMap; } set { _emissiveMap = value; } }

        public void SetSprite(SpriteRenderer spriteRenderer) {
            var matBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(matBlock);
            spriteRenderer.sprite = _sprite;
            matBlock.SetTexture("_MainTex", _sprite.texture);
            if (_normalMap != null) {
                matBlock.SetTexture("_BumpMap", _normalMap);
            }
            if (_emissiveMap != null) {
                matBlock.SetTexture("_EmissionMap", _emissiveMap);
            }
            spriteRenderer.SetPropertyBlock(matBlock);
        }
    }
}