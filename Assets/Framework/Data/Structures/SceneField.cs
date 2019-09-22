using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace PixelComrades {
        [Serializable]
    public class SceneField {

        [SerializeField] private Object _sceneAsset = null;
        [SerializeField] private string _sceneName = "";
        [SerializeField] private int _buildIndex = -1;

        public string SceneName { get { return _sceneName; } }
        public Object SceneAsset { get { return _sceneAsset; } }

        public int BuildIndex {
            get {
                if (_buildIndex < 0) {
                    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                        var scene = SceneUtility.GetScenePathByBuildIndex(i);
                        scene = scene.Remove(scene.LastIndexOf(".", StringComparison.InvariantCulture));
                        scene = scene.Remove(0, 7);
                        if (scene == _sceneName) {
                            _buildIndex = i;
                            break;
                        }
                    }
                }
                return _buildIndex;
            }
        }

            // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField) {
            return sceneField.SceneName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))] public class SceneFieldPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            var sceneAsset = property.FindPropertyRelative("_sceneAsset");
            var sceneName = property.FindPropertyRelative("_sceneName");
            var sceneIdx = property.FindPropertyRelative("_buildIndex");
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (sceneAsset != null) {
                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
                if (EditorGUI.EndChangeCheck()) {
                    sceneAsset.objectReferenceValue = value;
                    if (sceneAsset.objectReferenceValue != null) {
                        var scenePath = AssetDatabase.GetAssetPath(sceneAsset.objectReferenceValue);
                        //var assetsIndex = scenePath.IndexOf("Assets", StringComparison.Ordinal) + 7;
                        //var extensionIndex = scenePath.LastIndexOf(".unity", StringComparison.Ordinal);
                        //scenePath = scenePath.Substring(assetsIndex, extensionIndex - assetsIndex);
                        scenePath = scenePath.Remove(scenePath.LastIndexOf(".", StringComparison.InvariantCulture));
                        scenePath = scenePath.Remove(0, 7);
                        sceneName.stringValue = scenePath;
                        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                            var scene = SceneUtility.GetScenePathByBuildIndex(i);
                            if (scene== scenePath) {
                                sceneIdx.intValue = i;
                            }
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}