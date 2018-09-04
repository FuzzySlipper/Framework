using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject {
    private static T _main;
    private static object _lock = new object();
    public static T Main {
        get {
            if (_main != null) {
                return _main;
            }
            lock (_lock) {
                if (_main == null) {
                    _main = Resources.Load<T>(typeof(T).Name);
                    if (_main == null) {
                        _main = CreateInstance<T>();
#if UNITY_EDITOR
                        var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/GameData/System/Resources/" + typeof(T).Name + ".asset");
                        AssetDatabase.CreateAsset(_main, assetPathAndName);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
#endif
                        if (_main is ICreationListener creationListener) {
                            creationListener.OnCreate();
                        }
                    }
                }
                return _main;
            }
        }
        protected set { _main = value; }
    }
}

public interface ICreationListener {
    void OnCreate();
}