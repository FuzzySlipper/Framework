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
    public class SpriteAnimationAssetReference : AssetReferenceT<SpriteAnimation> {
        public SpriteAnimationAssetReference(string guid) : base(guid) {
        }
    }
}
