using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    [CustomPropertyDrawer(typeof(NumberReference), true)]
    public class NumberReferenceDrawer : PropertyDrawer {
        
        private GUIStyle _popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var propertyInstance = fieldInfo.GetValue(property.serializedObject.targetObject) as NumberReference;
            if (propertyInstance == null) {
                return;
            }
            var floatRef = propertyInstance as FloatReference;
            if (floatRef == null) {
                SimpleDrawer(position, property, label, propertyInstance);
                return;
            }
            var floatRange = floatRef.Variable as FloatRangeVariable;
            if (floatRange != null) {
                RangeDrawer(position, property, label, propertyInstance);
            }
            else {
                SimpleDrawer(position, property, label, propertyInstance);
            }
        }

        private void SimpleDrawer(Rect position, SerializedProperty property, GUIContent label, NumberReference propertyInstance) {
            if (_popupStyle == null) {
                _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                _popupStyle.imagePosition = ImagePosition.ImageOnly;
            }
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.BeginChangeCheck();

            var modifier = property.FindPropertyRelative("Modifier");
            var constantValue = property.FindPropertyRelative("ConstantMin");
            var variable = property.FindPropertyRelative("Variable");

            var buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            modifier.enumValueIndex = (int) (ValueModifier) EditorGUI.EnumPopup(buttonRect, (ValueModifier) modifier.enumValueIndex, _popupStyle);
            var mod = (ValueModifier) modifier.enumValueIndex;
            if (mod == ValueModifier.Absolute) {
                EditorGUI.PropertyField(
                    position,
                    constantValue,
                    GUIContent.none);
            }
            else if (mod == ValueModifier.AbsoluteRange) {
                position.width *= 0.5f;
                var pos = position.position;
                EditorGUI.PropertyField(
                    position,
                    constantValue,
                    GUIContent.none);
                pos.x += position.width;
                position.position = pos;
                EditorGUI.PropertyField(
                    position,
                    property.FindPropertyRelative("ConstantMax"),
                    GUIContent.none);
            }
            else {
                position.width *= mod == ValueModifier.Reference ? 0.5f : 0.34f;
                var pos = position.position;
                if (mod != ValueModifier.Reference) {
                    EditorGUI.PropertyField(
                        position,
                        constantValue,
                        GUIContent.none);
                    pos.x += position.width;
                }
                position.position = pos;
                EditorGUI.PropertyField(
                    position,
                    variable,
                    GUIContent.none);
                pos.x += position.width;
                position.position = pos;
                EditorGUI.LabelField(position, propertyInstance.StringValue);
            }
            if (EditorGUI.EndChangeCheck()) {
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private void RangeDrawer(Rect position, SerializedProperty property, GUIContent label, NumberReference propertyInstance) {
            if (_popupStyle == null) {
                _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                _popupStyle.imagePosition = ImagePosition.ImageOnly;
            }
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.BeginChangeCheck();

            var modifier = property.FindPropertyRelative("Modifier");
            var constantMin = property.FindPropertyRelative("ConstantMin");
            var constantMax = property.FindPropertyRelative("ConstantMax");
            var variable = property.FindPropertyRelative("Variable");

            var buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            modifier.enumValueIndex = (int) (ValueModifier) EditorGUI.EnumPopup(buttonRect, (ValueModifier) modifier.enumValueIndex, _popupStyle);
            var mod = (ValueModifier) modifier.enumValueIndex;
            var pos = position.position;
            if (mod == ValueModifier.Reference || mod == ValueModifier.Absolute) {
                position.width *= 0.5f;
            }
            else {
                position.width *= 0.2f;
            }
            if (mod != ValueModifier.Reference) {
                position.position = pos;
                EditorGUI.PropertyField(
                    position,
                    constantMin,
                    GUIContent.none);
                pos.x += position.width;
                position.position = pos;
                EditorGUI.PropertyField(
                    position,
                    constantMax,
                    GUIContent.none);
            }
            if (mod != ValueModifier.Absolute) {
                if (mod != ValueModifier.Reference) {
                    pos.x += position.width;
                    position.width *= 1.25f;
                }
                position.position = pos;
                EditorGUI.PropertyField(
                    position,
                    variable,
                    GUIContent.none);
                pos.x += position.width;
                position.position = pos;
                EditorGUI.LabelField(position, propertyInstance.StringValue);
            }
            if (EditorGUI.EndChangeCheck()) {
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}