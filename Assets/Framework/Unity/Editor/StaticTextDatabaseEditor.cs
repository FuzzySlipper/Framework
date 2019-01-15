using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(StaticTextDatabase), true)]
    public class StaticTextDatabaseEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            var database = (StaticTextDatabase) target;
            if (GUILayout.Button("Check For Data")) {
                database.AllText.Clear();
                var sFolderPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Assets\\" + database.CustomSearchPath;
                AddDataInPath(database, sFolderPath);
            }
            if (GUILayout.Button("Clear Data")) {
                database.AllText.Clear();
            }
            base.OnInspectorGUI();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("Tracking " + database.AllText.Count, MessageType.Info);
            serializedObject.ApplyModifiedProperties();
        }

        private void AddDataInPath(StaticTextDatabase database, string path) {
            var sDataPath = Application.dataPath;
            var dirName = Directory.GetDirectories(path);
            var directories = new List<string>();
            directories.AddRange(dirName);
            foreach (var directory in directories)
                AddDataInPath(database,directory);

            // get the system file paths of all the files in the asset folder
            var aFilePaths = Directory.GetFiles(path, "*.asset");

            // enumerate through the list of files loading the assets they represent and getting their type		
            foreach (var sFilePath in aFilePaths) {
                var sAssetPath = sFilePath.Substring(sDataPath.Length - 6);

                var data = AssetDatabase.LoadAssetAtPath(sAssetPath, typeof(StaticTextHolder)) as StaticTextHolder;
                if (data == null) {
                    continue;
                }
                database.AllText.Add(data);
            }
            EditorUtility.SetDirty(this);
        }
    }
}
