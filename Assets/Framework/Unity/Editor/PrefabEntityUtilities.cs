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
        private static HashSet<int> _ids = new HashSet<int>();

        [MenuItem("Tools/Generate Prefab Ids")]
        public static void GeneratePrefabIds() {
            _processedObjects.Clear();
            _ids.Clear();
            AddDataInPath(Application.dataPath + "\\GameData\\");
        }

        [MenuItem("Tools/Generate Art Ids")]
        public static void GenerateArtIds() {
            _processedObjects.Clear();
            _ids.Clear();
            AddDataInPath(Application.dataPath + "\\Art\\");
        }

        private static void AddDataInPath(string folderPath) {
            var dataPath = Application.dataPath;
            var dirName = Directory.GetDirectories(folderPath);
            var directories = new List<string>();
            directories.AddRange(dirName);
            foreach (var directory in directories) {
                AddDataInPath(directory);
            }
            var filePath = Directory.GetFiles(folderPath, "*.*");
            foreach (var sFilePath in filePath) {
                var assetPath = sFilePath.Substring(dataPath.Length - 6);
                var data = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                if (data == null) {
                    continue;
                }
                var go = data as GameObject;
                if (go == null || go.CompareTag(StringConst.TagDummy)) {
                    continue;
                }
                if (_processedObjects.Contains(data)) {
                    continue;
                }
                var prefabEntity = go.GetComponent<PrefabEntity>();
                if (prefabEntity == null) {
                    continue;
                }
                assetPath = AssetDatabase.GetAssetPath(go);
                _processedObjects.Add(data);
                assetPath = Regex.Replace(assetPath, ".prefab", "");
                var resourceIndex = assetPath.IndexOf(ResourcesPath);
                if (resourceIndex != -1) {
                    assetPath = assetPath.Substring(assetPath.IndexOf(ResourcesPath));
                    assetPath = Regex.Replace(assetPath, ResourcesPath, "");
                }
                prefabEntity.SetId(ref _ids, assetPath);
                EditorUtility.SetDirty(go);
            }
        }

        public static void CheckForModifiedPrefabs() {
            _processedObjects.Clear();
            _ids.Clear();
            AddDataInPathIfNeeded(Application.dataPath + "\\GameData\\");
        }

        private static void AddDataInPathIfNeeded(string folderPath) {
            var dataPath = Application.dataPath;
            var dirName = Directory.GetDirectories(folderPath);
            var directories = new List<string>();
            directories.AddRange(dirName);
            foreach (var directory in directories) {
                AddDataInPath(directory);
            }
            var filePath = Directory.GetFiles(folderPath, "*.*");
            foreach (var sFilePath in filePath) {
                var assetPath = sFilePath.Substring(dataPath.Length - 6);
                var data = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                if (data == null) {
                    continue;
                }
                var go = data as GameObject;
                if (go == null || go.CompareTag(StringConst.TagDummy)) {
                    continue;
                }
                if (_processedObjects.Contains(data)) {
                    continue;
                }
                var prefabEntity = go.GetComponent<PrefabEntity>();
                if (prefabEntity == null) {
                    continue;
                }
                assetPath = AssetDatabase.GetAssetPath(go);
                _processedObjects.Add(data);
                assetPath = Regex.Replace(assetPath, ".prefab", "");
                var resourceIndex = assetPath.IndexOf(ResourcesPath);
                if (resourceIndex != -1) {
                    assetPath = assetPath.Substring(assetPath.IndexOf(ResourcesPath));
                    assetPath = Regex.Replace(assetPath, ResourcesPath, "");
                }
                if (prefabEntity.ResourcePath == assetPath && !_ids.Contains(prefabEntity.PrefabId)) {
                    _ids.Add(prefabEntity.PrefabId);
                    continue;
                }
                Debug.LogFormat("{0} vs {1}", assetPath, prefabEntity.ResourcePath);
                prefabEntity.SetId(ref _ids, assetPath);
                //EditorUtility.SetDirty(go);
            }
        }

    }
}