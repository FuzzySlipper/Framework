using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class ScriptableDatabases {
        
        private static Dictionary<System.Type, ScriptableDatabase> _databases = new Dictionary<Type, ScriptableDatabase>();
        private static bool _setup = false;
        
        private const string EditorFolder = "Assets/GameData/Resources/";

        static ScriptableDatabases() {
            Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Init() {
            if (_setup) {
                return;
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var allTypes = assemblies[a].GetTypes();
                for (int t = 0; t < allTypes.Length; t++) {
                    var checkTyped = allTypes[t];
                    if (!checkTyped.InheritsFrom(typeof(ScriptableDatabase<>)) || checkTyped.IsAbstract) {
                        continue;
                    }
                    var db = Resources.Load(checkTyped.Name);
                    if (db == null) {
                        db = UnityEngine.ScriptableObject.CreateInstance(checkTyped);
                        if (db == null) {
                            Debug.Log("Bad class " + checkTyped.Name);
                            continue;
                        }
#if UNITY_EDITOR
                        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(EditorFolder + checkTyped.Name + ".asset");
                        AssetDatabase.CreateAsset(db, assetPathAndName);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
#endif
                    }
                    _databases.AddOrUpdate(checkTyped, db as ScriptableDatabase);
                }
            }
            _setup = true;
        }
        
        public static void Register<T>(ScriptableDatabase db) where T : ScriptableDatabase {
            _databases.AddOrUpdate(typeof(T), db);
        }

        public static T GetDatabase<T>() where T : ScriptableDatabase<T> {
            var type = typeof(T);
            return _databases.TryGetValue(type, out var db) ? db as T : null;
        }

        public static ScriptableDatabase GetDatabase(Type type) {
            return _databases.TryGetValue(type, out var db) ? db : null;
        }
    }

    public abstract class ScriptableDatabase : ScriptableObject {
        public abstract Type DbType { get; }
        public abstract IEnumerable<UnityEngine.Object> AllObjects { get; }
        public abstract T GetObject<T>(string id) where T : Object;
        public abstract void AddObject(UnityEngine.Object obj);
        public abstract void CleanObjectList();

        public virtual string GetId<T>(T obj) where T : Object {
            return obj != null ? obj.name : "";
;        }


#if UNITY_EDITOR
        public virtual System.Object GetEditorWindow() {
            return null;
        }
#endif
    }

    public abstract class ScriptableDatabase<TV> : ScriptableDatabase where TV : ScriptableDatabase<TV> {

        private const string EditorFolder = "Assets/GameData/Resources/";
        
        private static TV _main;
        public static TV Main {
            get {
                if (_main != null) {
                    return _main;
                }
                if (_main == null) {
                    _main = ScriptableDatabases.GetDatabase<TV>();
                }
                return _main;
            }
            protected set { _main = value; }
        }
    }

    public abstract class SimpleScriptableDatabase<TV> : ScriptableDatabase<TV> where TV : ScriptableDatabase<TV> {
        public override IEnumerable<UnityEngine.Object> AllObjects { get { return null; } }
        public override void AddObject(Object obj) { }
        public override Type DbType { get { return null; } }
        public override void CleanObjectList() { }
        public override T GetObject<T>(string id) {
            return null;
        }
    }

    public interface ICustomPreview {
        UnityEngine.Object Preview { get; }
        UnityEngine.Object EditorObject { get; }
    }

    public static class CustomPreviewExtension {
        
        public static Texture GetPreviewTexture(this ICustomPreview x) {
            if (x == null) {
                return null;
            }
            var preview = x.Preview;
            if (preview == null) {
                return null;
            }
#if UNITY_EDITOR
            return Sirenix.Utilities.Editor.GUIHelper.GetAssetThumbnail(preview, preview.GetType(), true);
#else
            return null;
#endif
        }
    }
}
