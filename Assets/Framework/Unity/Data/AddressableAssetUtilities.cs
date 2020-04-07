using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PixelComrades {
    public static class AddressableAssetUtilities {}

    [Serializable]
    public class PrefabAssetReference : AssetReferenceT<GameObject> {
        public PrefabAssetReference(string guid) : base(guid) {}
    }

    [Serializable]
    public class AudioClipAssetReference : AssetReferenceT<AudioClip> {
        public AudioClipAssetReference(string guid) : base(guid) {
        }
    }

    [Serializable]
    public class TextureAssetReference : AssetReferenceT<Texture2D> {
        public TextureAssetReference(string guid) : base(guid) {
        }
    }

    [Serializable]
    public class ActionFxAssetReference : AssetReferenceT<ActionFx> {
        public ActionFxAssetReference(string guid) : base(guid) {
        }
    }

    [Serializable]
    public class SpriteAnimationReference : AssetReference {
        [SerializeField] public bool IsDirectional;

        public SpriteAnimationReference(string guid) : base(guid) {
        }

        public override bool ValidateAsset(UnityEngine.Object obj) {
            if (obj is SpriteAnimation) {
                return true;
            }
            if (obj is SpriteAnimationSet) {
                return true;
            }
            return false;
        }

        public override void ReleaseAsset() {
            if (Asset == null) {
                return;
            }
            base.ReleaseAsset();
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
