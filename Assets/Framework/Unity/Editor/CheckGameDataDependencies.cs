using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace PixelComrades {
    public static class CheckGameDataDependencies {

        [MenuItem("Tools/Check GameData Dependencies #&g")]
        [MenuItem("Assets/Check GameData Dependencies", false, 50)]
        public static void CheckDependencies() {
            Object[] roots = new Object[Selection.objects.Length];
            for (int i = 0; i < roots.Length; i++) {
                roots[i] = Selection.objects[i];
            }
            List<Object> newSelection = new List<Object>();
            var dependencies = EditorUtility.CollectDependencies(roots);
            for (int i = 0; i < dependencies.Length; i++) {
                if (dependencies[i] is Shader) {
                    continue;
                }
                if (dependencies[i] is MonoScript) {
                    continue;
                }
                var path = AssetDatabase.GetAssetPath(dependencies[i]);
                if (!path.Contains("GameData") && !path.Contains("resources") && !path.Contains("Font") && !path.Contains("Resources")) {
                    Debug.Log(dependencies[i].name + " not in GameData. Located at " + path);
                    newSelection.Add(dependencies[i]);
                }
            }
            if (newSelection.Count != 0) {
                Selection.objects = newSelection.ToArray();
            }
        }
    }
}
