using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(ScriptableDatabase), true)]
    public class ScriptableDatabaseEditor : OdinEditor {
        private const string EditorFolder = "Assets/GameData/Resources/";
        
        public override void OnInspectorGUI() {
            var script = (ScriptableDatabase) target;
            if (GUILayout.Button("Add All Types")) {
                ScriptableDatabaseEditorExtension.RefreshDbProjectAssets(script);
            }
            if (GUILayout.Button("Save Json")) {
                string path = EditorUtility.SaveFilePanel("DB Backup location", Application.streamingAssetsPath , script.name, "json");
                if (path.Length == 0) {
                    return;
                }
                var serialized = JsonConvert.SerializeObject(new SerializedScriptableObjectCollection(script, script.AllObjects), 
                Formatting
                .Indented, 
                Serializer.ConverterSettings);
                // var jsonOutput = EditorJsonUtility.ToJson(target, true);
                // var sb = new System.Text.StringBuilder(jsonOutput);
                // foreach (var allObject in script.AllObjects) {
                //     sb.AppendNewLine();
                // }
                // sb.AppendNewLine();
                FileUtility.SaveFile(path, serialized);
            }
            if (GUILayout.Button("Load Json")) {
                string path = EditorUtility.OpenFilePanel("DB Backup location", Application.streamingAssetsPath, "json");
                if (path.Length == 0) {
                    return;
                }
                // var loaded = FileUtility.ReadFile(path);
                // var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(EditorFolder + script.GetType().Name + ".asset");
                // var created = CreateInstance(script.GetType());
                // AssetDatabase.CreateAsset(created, assetPathAndName);
                // AssetDatabase.SaveAssets();
                // AssetDatabase.Refresh();
                //EditorJsonUtility.FromJsonOverwrite(loaded, created);
                var converted = JsonConvert.DeserializeObject<SerializedScriptableObjectCollection>(FileUtility.ReadFile(path), Serializer
                .ConverterSettings);
                converted.Restore();
                var main = converted.Main.Value as ScriptableDatabase;
                if (main == null || converted.Children == null) {
                    return;
                }
                for (int i = 0; i < converted.Children.Count(); i++) {
                    main.AddObject(converted.Children[i].Value);
                }
            }
            base.OnInspectorGUI();
        }
    }

    public static class ScriptableDatabaseEditorExtension {
        public static void RefreshDbProjectAssets(ScriptableDatabase db) {
            var ids = AssetDatabase.FindAssets(string.Format("t:{0}", db.DbType));
            for (int i = 0; i < ids.Length; i++) {
                var obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[i]), db.DbType);
                if (obj != null) {
                    db.AddObject(obj);
                }
            }
            EditorUtility.SetDirty(db);
        }

        
    }

    public class CustomPreviewDrawer<TItem> : OdinValueDrawer<TItem> where TItem : ICustomPreview {
        
        protected override void DrawPropertyLayout(GUIContent label) {
            ICustomPreview item = this.ValueEntry.SmartValue;
<<<<<<< HEAD
            if (item == null || item.EditorObject == null) {
                return;
            }
=======
            // if (item == null || item.EditorObject == null) {
            //     base.DrawPropertyLayout(label);
            //     return;
            // }
>>>>>>> FirstPersonAction
            var rect = EditorGUILayout.GetControlRect(label != null, 45);
            if (label != null) {
                rect.xMin = EditorGUI.PrefixLabel(rect.AlignCenterY(15), label).xMin;
            }
            else {
                rect = EditorGUI.IndentedRect(rect);
            }
<<<<<<< HEAD
            var texture = GUIHelper.GetAssetThumbnail(item.Preview, typeof(Sprite), true);
            GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : item.EditorObject.name);
            ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(rect.AlignLeft(45), item.EditorObject, texture, ValueEntry.BaseValueType);
=======
            Texture2D texture = null;
            string labelValue = "None";
            UnityEngine.Object targetObj = null;
            if (item != null && item.EditorObject != null) {
                texture = GUIHelper.GetAssetThumbnail(item.Preview, typeof(Sprite), true);
                targetObj = item.EditorObject;
                labelValue = targetObj.name;
            }
            GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : labelValue);
            ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(rect.AlignLeft(45), targetObj, texture, ValueEntry.BaseValueType);
>>>>>>> FirstPersonAction
        }
    }
}