using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PixelComrades {
    public static class AddressableAssetUtilities {}

    [System.Serializable]
    public abstract class AssetEntry {
        public AssetReference AssetReference = new AssetReference();
        public string Path;
        public abstract System.Type Type { get; }
        public abstract UnityEngine.Object Asset { get; }
    }

    [System.Serializable]
    public class AssetEntry<T> : AssetEntry where T : UnityEngine.Object {
        public override Type Type { get { return typeof(T); } }
#if UNITY_EDITOR
        public override UnityEngine.Object Asset { get { return AssetReference.editorAsset; } }
#else
        public override UnityEngine.Object Asset { get { return AssetReference.Asset; } }

#endif
        public AsyncOperationHandle<T> LoadAssetAsync() {
            return AssetReference.LoadAssetAsync<T>();
        }

        public void ReleaseAsset() {
            AssetReference.ReleaseAsset();
        }
    }

    [System.Serializable]
    public class GameObjectReference : AssetEntry<GameObject> {}
    [System.Serializable]
    public class MaterialReference : AssetEntry<Material> {}
    [System.Serializable]
    public class SpriteReference : AssetEntry<Sprite> {}
    [System.Serializable]
    public class AudioClipReference : AssetEntry<AudioClip> {}
    [System.Serializable]
    public class TextureAssetReference : AssetEntry<Texture2D> {}
    [System.Serializable]
    public class ActionFxReference : AssetEntry<ActionFx> {}
  
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
