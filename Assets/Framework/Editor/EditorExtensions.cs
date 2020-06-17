using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using Object = UnityEngine.Object;

namespace PixelComrades {
    
    public static class EditorExtensions {
        
        private static GUIStyle _listStyle = new GUIStyle("ObjectPickerBackground");

        public static bool DisplayHolderList<T,TV,TX>(SerializedObject serializedObject, string propertyName, string[] labels,  ref TX[] 
        array) where TX : GenericAssetHolder<T,TV>, new() where TV : Object where T : AssetReferenceEntry<TV>, new() {
            if (array.Length != labels.Length) {
                System.Array.Resize(ref array, labels.Length);
                for (int i = 0; i < array.Length; i++) {
                    if (array[i] == null) {
                        array[i] = new TX() {
                            Curve = AnimationCurve.Linear(0, 0, 1, 1)
                        };
                    }
                }
            }
            bool changed = false;
            EditorGUILayout.BeginVertical(_listStyle);
            EditorGUILayout.LabelField(propertyName);
            SerializedProperty listProperty = serializedObject.FindProperty(propertyName);
            EditorGUI.indentLevel++;
            // var labelStyle = new GUIStyle("ToolbarButton");
            for (int l = 0; l < listProperty.arraySize; l++) {
                EditorGUILayout.BeginVertical();
                SerializedProperty entryProperty = listProperty.GetArrayElementAtIndex(l);
                //EditorGUILayout.LabelField(labels[l], labelStyle);
                //EditorGUILayout.Space();
                //EditorGUILayout.PropertyField(elementProperty, new GUIContent(LevelMeshObjects.ValueAt(i)), true);
                var objectsProperty = entryProperty.FindPropertyRelative("Objects");
                var objTarget = ((List<T>) objectsProperty.GetTargetObjectOfProperty());
                for (int i = 0; i < objectsProperty.arraySize; i++) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(objectsProperty.GetArrayElementAtIndex(i), new GUIContent(labels[l]), true);
                    if (GUILayout.Button("X", GUILayout.MaxWidth(30))) {
                        objTarget.RemoveAt(i);
                        changed = true;
                        //objectsProperty.DeleteArrayElementAtIndex(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(entryProperty.FindPropertyRelative("Curve"), GUIContent.none, true);
                if (GUILayout.Button("Add")) {
                    objTarget.Add(new T());
                    changed = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            return changed;
        }
        
        public static void DisplayRangeList<T>(SerializedObject serializedObject, string soName, int min, int max, List<string> labels, ref T[] array) {
            if (array.Length != labels.Count) {
                System.Array.Resize(ref array, labels.Count);
            }
            DisplayRangeList(serializedObject, soName, min, max , labels);
        }

        public static void DisplayRangeList(SerializedObject serializedObject, string soName, int min, int max, List<string> labels) {
            
            EditorGUILayout.BeginVertical(_listStyle);
            EditorGUILayout.LabelField(soName);
            SerializedProperty listProperty = serializedObject.FindProperty(soName);
            EditorGUI.indentLevel++;
            var labelStyle = new GUIStyle("ToolbarButton");
            for (int l = 0; l < labels.Count; l++) {
                if (listProperty.arraySize <= l) {
                    break;
                }
                EditorGUILayout.Space();
                SerializedProperty entryProperty = listProperty.GetArrayElementAtIndex(l);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labels[l], labelStyle);
                EditorGUILayout.IntSlider(entryProperty, min, max, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }

        // public static void DisplayList(SerializedObject serializedObject, string soName, List<string> labels) {
        //
        //     EditorGUILayout.BeginVertical(_listStyle);
        //     EditorGUILayout.LabelField(soName);
        //     SerializedProperty listProperty = serializedObject.FindProperty(soName);
        //     EditorGUI.indentLevel++;
        //     var labelStyle = new GUIStyle("ToolbarButton");
        //     for (int l = 0; l < listProperty.arraySize; l++) {
        //         EditorGUILayout.Space();
        //         SerializedProperty entryProperty = listProperty.GetArrayElementAtIndex(l);
        //         EditorGUILayout.BeginHorizontal();
        //         EditorGUILayout.LabelField(labels[l], labelStyle);
        //         EditorGUILayout.PropertyField(entryProperty);
        //         EditorGUILayout.EndHorizontal();
        //         EditorGUILayout.Space();
        //     }
        //     EditorGUI.indentLevel--;
        //     serializedObject.ApplyModifiedProperties();
        //     EditorGUILayout.EndVertical();
        // }

        public static void DisplayList<T>(SerializedObject serializedObject, string soName) where T : struct, IConvertible {
            // var length = EnumHelper.GetLength<T>();
            // EditorGUILayout.BeginVertical(_listStyle);
            // EditorGUILayout.LabelField(soName);
            // SerializedProperty listProperty = serializedObject.FindProperty(soName);
            // if (listProperty.arraySize != length) {
            //     listProperty.arraySize = length;
            // }
            // EditorGUI.indentLevel++;
            // for (int l = 0; l < listProperty.arraySize; l++) {
            //     EditorGUILayout.Space();
            //     SerializedProperty entryProperty = listProperty.GetArrayElementAtIndex(l);
            //     EditorGUILayout.BeginHorizontal();
            //     //EditorGUILayout.LabelField(, labelStyle);
            //     EditorGUILayout.PropertyField(entryProperty, new GUIContent(EnumHelper.GetString<T>(l)));
            //     EditorGUILayout.EndHorizontal();
            //     EditorGUILayout.Space();
            // }
            // EditorGUI.indentLevel--;
            // serializedObject.ApplyModifiedProperties();
            // EditorGUILayout.EndVertical();
            DisplayList(serializedObject, soName, Enum.GetNames(typeof(T)));
        }

        public static void DisplayList(SerializedObject serializedObject, string soName, IList<string> labels) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(soName);
            SerializedProperty listProperty = serializedObject.FindProperty(soName);
            if (listProperty.arraySize != labels.Count) {
                listProperty.arraySize = labels.Count;
            }
            EditorGUI.indentLevel++;
            for (int l = 0; l < listProperty.arraySize; l++) {
                EditorGUILayout.Space();
                SerializedProperty entryProperty = listProperty.GetArrayElementAtIndex(l);
                EditorGUILayout.BeginHorizontal(_listStyle);
                //EditorGUILayout.LabelField(, labelStyle);
                EditorGUILayout.PropertyField(entryProperty, new GUIContent(labels[l]));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }
        
    }

    public static class AddressableAssetEditorUtility {
    
        public static AddressableAssetEntry GetOrCreateEntry(Object o) {
            AddressableAssetSettings aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = null;
            bool foundAsset = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out var guid, out long _);
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (foundAsset && (path.ToLower().Contains("assets"))) {
                if (aaSettings != null) {
                    entry = aaSettings.FindAssetEntry(guid);
                }
            }
            if (entry != null) {
                return entry;
            }
            entry = aaSettings.CreateOrMoveEntry(guid, aaSettings.DefaultGroup);
            return entry;
        }
    }

    public abstract class ModalWindow : EditorWindow {

        public enum WindowResult {
            None,
            Ok,
            Cancel,
            Invalid,
            LostFocus
        }

        private const float Titlebar = 18;
        
        private WindowResult _result = WindowResult.None;
        protected System.Action<ModalWindow> Del;

        public WindowResult Result { get { return _result; } }

        protected virtual void OnLostFocus() {
            _result = WindowResult.LostFocus;

            if (Del != null)
                Del(this);
        }

        protected virtual void Cancel() {
            _result = WindowResult.Cancel;

            if (Del != null)
                Del(this);
            Close();
        }

        protected virtual void Ok() {
            _result = WindowResult.Ok;

            if (Del != null)
                Del(this);
            Close();
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label(titleContent);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Rect content = new Rect(0, Titlebar, position.width, position.height - Titlebar);
            Draw(content);
        }

        protected abstract void Draw(Rect region);
    }
    ///     <summary>
    /// The rename popup is a generic popup that allow the user to input a name or to rename an existing one.
    /// You can pass a delegate to valide the currently input string.
    /// </summary>
    public class RenameWindow : ModalWindow  {
     
        public const float FieldHeight = 20;
        public const float Height = 56;
        public const float Width = 250;
     
        private string _text;
        public string Text{ get { return _text; }}

        public static RenameWindow Open(System.Action<ModalWindow> owner, string title, string text) {
            return Open(owner, title, text, GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
        }
     
        public static RenameWindow Open(System.Action<ModalWindow> owner, string title, string text, Vector2 position) {
            RenameWindow renameWindow = RenameWindow.CreateInstance<RenameWindow>();
     
            renameWindow.Del = owner;
            renameWindow.titleContent = new GUIContent(title);
            renameWindow._text = text;
     
            float halfWidth = Width / 2;
     
            float x = position.x - halfWidth;
            float y = position.y;
     
            float height = Height + (FieldHeight);
     
            Rect rect = new Rect(x, y, 0, 0);
            renameWindow.position = rect;
            renameWindow.ShowAsDropDown(rect, new Vector2(Width, height));
     
            return renameWindow;
        }
     
        protected override void Draw(Rect region) {
     
            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.Return)
                    Ok();
     
                if (Event.current.keyCode == KeyCode.Escape)
                    Cancel();
            }
     
            GUILayout.BeginArea(region);
     
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUI.color = Color.white;
            _text = EditorGUILayout.TextField(_text);

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
     
            GUILayout.BeginHorizontal();
     
            if (GUILayout.Button("Ok"))
                Ok();
     
            if (GUILayout.Button("Cancel"))
                Cancel();
     
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
