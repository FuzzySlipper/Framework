using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace PixelComrades {
    //[InitializeOnLoad]
    public static class PrefabEntityUtilities {

        //TODO: enable this later
        //static PrefabEntityUtilities() {
        //    CheckForModifiedPrefabs();
        //} 

        private const string ResourcesPath = "Resources/";
        private static HashSet<Object> _processedObjects = new HashSet<Object>();
        private static HashSet<string> _ids = new HashSet<string>();

        [MenuItem("Tools/Generate Prefab Ids")]
        public static void GeneratePrefabIds() {
            _processedObjects.Clear();
            _ids.Clear();
            CheckIds();
        }

        private static void CheckIds() {
            var ids = AssetDatabase.FindAssets("t:PrefabEntity");
            for (int i = 0; i < ids.Length; i++) {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(ids[i]));
                if (obj == null) {
                    continue;
                }
                var prefabEntity = obj.GetComponent<PrefabEntity>();
                if (prefabEntity != null) {
                    prefabEntity.SetId(ref _ids, ids[i]);
                }
            }
        }

    }
}