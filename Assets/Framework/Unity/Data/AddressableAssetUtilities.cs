using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;


namespace PixelComrades {
    public static class AddressableAssetUtilities {}
    
    [Serializable]
    public class SpriteAssetReference : AssetReferenceT<Sprite> {
        public SpriteAssetReference(string guid) : base(guid) {}
    }

    [Serializable]
    public class TextureAssetReference : AssetReferenceT<Texture2D> {
        public TextureAssetReference(string guid) : base(guid) {
        }
    }

    [Serializable]
    public class SpriteReference : AssetReference {
        [SerializeField] public bool IsSprite;

        public override bool ValidateAsset(UnityEngine.Object obj) {
            if (obj is SpriteAnimation) {
                return true;
            }
            if (obj is SpriteAnimationSet) {
                return true;
            }
            if (obj is Sprite) {
                return true;
            }
            return false;
        }
#if UNITY_EDITOR
        public override bool SetEditorAsset(UnityEngine.Object value) {
            if (value != null) {
                if (!(value is SpriteAnimation) && !(value is SpriteAnimationSet) && !(value is Sprite)) {
                    return false;
                }
            }
            if (!base.SetEditorAsset(value)) {
                return false;
            }
            if (value != null && value is Sprite) {
                IsSprite = true;
            }
            else {
                IsSprite = false;
            }
            return true;
        }

        public override bool SetEditorSubObject(UnityEngine.Object value) {
            if (!base.SetEditorSubObject(value)) {
                return false;
            }
            if (value != null && value is Sprite) {
                IsSprite = true;
            }
            else {
                IsSprite = false;
            }
            return true;
        }
#endif
    }

    [Serializable]
    public class SpriteAnimationReference : AssetReference {
        [SerializeField] public bool IsDirectional;
        
        public override bool ValidateAsset(UnityEngine.Object obj) {
            if (obj is SpriteAnimation) {
                return true;
            }
            if (obj is SpriteAnimationSet) {
                return true;
            }
            return false;
        }
#if UNITY_EDITOR
        public override bool SetEditorAsset(UnityEngine.Object value) {
            if (value != null) {
                if (!(value is SpriteAnimation) && !(value is SpriteAnimationSet)) {
                    return false;
                }
            }
            if (!base.SetEditorAsset(value)) {
                return false;
            }
            if (value != null && value is DirectionalAnimation) {
                IsDirectional = true;
            }
            else {
                IsDirectional = false;
            }
            return true;
        }

        public override bool SetEditorSubObject(UnityEngine.Object value) {
            if (!base.SetEditorSubObject(value)) {
                return false;
            }
            if (value != null && value is DirectionalAnimation) {
                IsDirectional = true;
            }
            else {
                IsDirectional = false;
            }
            return true;
        }
#endif
    }
}
