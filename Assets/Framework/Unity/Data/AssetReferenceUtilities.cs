using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace PixelComrades {
    public static class AssetReferenceUtilities {
        public static void SetPath(AssetReferenceEntry entry) {
#if UNITY_EDITOR
            entry.Path = entry.Asset != null ? UnityEditor.AssetDatabase.GetAssetPath(entry.Asset) : "";
            if (!string.IsNullOrEmpty(entry.AssetReference.SubObjectName)) {
                entry.Path += StringConst.MultiEntryBreak + entry.AssetReference.SubObjectName;
            }
            // if (entry is SubAssetReferenceEntry subAsset) {
            //     subAsset.SubAssetName = entry.AssetReference.SubObjectName;
            // }
#endif 
        }

        public static Object LoadAsset(AssetReferenceEntry target) {
#if UNITY_EDITOR
            var pathSplit = target.Path.SplitFromEntryBreak();
            if (pathSplit == null) {
                return null;
            }
            if (pathSplit.Length == 2) {
                // }
                // if (target is SubAssetReferenceEntry subAsset) {
                var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(pathSplit[0]);
                if (objs.Length == 1) {
                    return objs[0];
                }
                for (int i = 0; i < objs.Length; i++) {
                    var obj = objs[i];
                    if (obj.name == pathSplit[1]) {
                        return obj;
                    }
                }
            }
            else {
                return UnityEditor.AssetDatabase.LoadAssetAtPath(target.Path, target.Type);
            }
#endif
            return null;
        }
    }

    [System.Serializable]
    public abstract class AssetReferenceEntry {
        public AssetReference AssetReference = new AssetReference();
        public string Path;
        public abstract System.Type Type { get; }
        public abstract UnityEngine.Object Asset { get; }
        public abstract bool IsLoaded { get; }
    }

    [System.Serializable]
    public class AssetReferenceEntry<T> : AssetReferenceEntry where T : UnityEngine.Object {
        private bool _isLoaded = false;

        private T _loadedAsset;
        private List<Action<T>> _dels = new List<Action<T>>();
        public override Type Type { get { return typeof(T); } }

#if UNITY_EDITOR
        public override UnityEngine.Object Asset { get { return AssetReference.editorAsset; } }
#else
        public override UnityEngine.Object Asset { get { return AssetReference.Asset; } }

#endif
        public T LoadedAsset {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return AssetReference.editorAsset as T;
                }
#endif
                return _loadedAsset;
            }
        }
        public override bool IsLoaded {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return true;
                }
#endif
                return _isLoaded;
            }
        }

        public void LoadAsset() {
            AssetReference.LoadAssetAsync<T>().Completed += CompleteLoad;
        }

        public void LoadAsset(Action<T> del) {
            if (IsLoaded) {
                del(_loadedAsset);
                return;
            }
            _dels.Add(del);
            LoadAsset();
        }

        private void CompleteLoad(AsyncOperationHandle<T> load) {
            _loadedAsset = load.Result;
            if (load.Status == AsyncOperationStatus.Failed) {
                Debug.LogErrorFormat("Failed Loading {0} {1}", Path, load.OperationException.Message);
                return;
            }
            if (load.Status == AsyncOperationStatus.None) {
                Debug.LogErrorFormat("Failed Loading {0} {1}", Path, load.OperationException.Message);
                return;
            }
            _isLoaded = true;
            for (int i = 0; i < _dels.Count; i++) {
                _dels[i](_loadedAsset);
            }
            _dels.Clear();
        }

        public void ReleaseAsset() {
            if (!_isLoaded) {
                return;
            }
            AssetReference.ReleaseAsset();
            _loadedAsset = null;
            _dels.Clear();
            _isLoaded = false;
        }
    }
//
//     [System.Serializable]
//     public abstract class SubAssetReferenceEntry : AssetReferenceEntry {
//         public string SubAssetName;
//     }
//
//     [System.Serializable]
//     public class SubAssetReferenceEntry<T> : SubAssetReferenceEntry where T : UnityEngine.Object {
//         public override Type Type { get { return typeof(T); } }
// #if UNITY_EDITOR
//         public override UnityEngine.Object Asset { get { return AssetReference.editorAsset; } }
// #else
//         public override UnityEngine.Object Asset { get { return AssetReference.Asset; } }
//
// #endif
//         public AsyncOperationHandle<T> LoadAssetAsync() {
//             return AssetReference.LoadAssetAsync<T>();
//         }
//
//         public void ReleaseAsset() {
//             AssetReference.ReleaseAsset();
//         }
//     }

    [System.Serializable]
    public class GameObjectReference : AssetReferenceEntry<GameObject> {}
    [System.Serializable]
    public class MaterialReference : AssetReferenceEntry<Material> {}
    [System.Serializable]
    public class SpriteReference : AssetReferenceEntry<Sprite> { }
    [System.Serializable]
    public class AudioClipReference : AssetReferenceEntry<AudioClip> {}
    [System.Serializable]
    public class TextureAssetReference : AssetReferenceEntry<Texture2D> {}
    [System.Serializable]
    public class ActionFxReference : AssetReferenceEntry<ActionFx> {}
    [System.Serializable]
    public class SpriteAnimationFrameReference : AssetReferenceEntry<Sprite> {
        public string SpriteAnimationName;
        public int Frame;
    }
    [Serializable]
    public class SpriteAnimationReference : AssetReferenceEntry<SpriteAnimation> {
        
        public bool ValidateAsset(UnityEngine.Object obj) {
            if (obj is SpriteAnimation) {
                return true;
            }
            if (obj is SpriteAnimationSet) {
                return true;
            }
            return false;
        }
    }
}
