using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace PixelComrades {
    [CustomEditor(typeof(ScriptableDatabase), true)]
    public class ScriptableDatabaseEditor : OdinEditor {
        public override void OnInspectorGUI() {
            var script = (ScriptableDatabase) target;
            if (GUILayout.Button("Add All Types")) {
                ScriptableDatabaseEditorExtension.RefreshDbProjectAssets(script);
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
            if (item == null) {
                return;
            }
            var rect = EditorGUILayout.GetControlRect(label != null, 45);
            if (label != null) {
                rect.xMin = EditorGUI.PrefixLabel(rect.AlignCenterY(15), label).xMin;
            }
            else {
                rect = EditorGUI.IndentedRect(rect);
            }
            var texture = GUIHelper.GetAssetThumbnail(item.Preview, typeof(Sprite), true);
            GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : item.EditorObject.name);
            ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(rect.AlignLeft(45), item.EditorObject, texture, ValueEntry.BaseValueType);
        }
    }
}