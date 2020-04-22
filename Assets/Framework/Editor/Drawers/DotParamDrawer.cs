using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PixelComrades {
    public abstract class BaseParamDrawer : PropertyDrawer {
        public const float V_PAD = 2f;
        public const float H_PAD = 10f;
        public const float TIME_VALUE_WIDHT = 35f;
        public const float TIME_LABEL_WIDHT = 15f;
        public const float TIME_FIELD_WIDHT = TIME_VALUE_WIDHT + TIME_LABEL_WIDHT + H_PAD;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight + 2f * V_PAD;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Time
            position.y += (GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight) / 2;
            position.width = TIME_VALUE_WIDHT;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Time"), GUIContent.none);

            position.x += TIME_VALUE_WIDHT;
            position.width = TIME_LABEL_WIDHT;
            EditorGUI.LabelField(position, "%");
        }
    }
}
