using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagAttribute))]
public class EnumFlagDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EnumFlagAttribute flagSettings = (EnumFlagAttribute)attribute;
		Enum targetEnum = (Enum)Enum.ToObject(fieldInfo.FieldType, property.intValue);

		GUIContent propName = new GUIContent(flagSettings.enumName);
		if (string.IsNullOrEmpty(flagSettings.enumName))
			propName = label;

		EditorGUI.BeginProperty(position, label, property);
		Enum enumNew = EditorGUI.EnumFlagsField(position, propName, targetEnum);
		property.intValue = (int)Convert.ChangeType(enumNew, fieldInfo.FieldType);
		EditorGUI.EndProperty();
	}

    public static void DrawDrawer(GUIContent label, SerializedProperty property, FieldInfo fieldInfo) {
		Enum targetEnum = (Enum)Enum.ToObject(fieldInfo.FieldType, property.intValue);

		//GUIContent propName = new GUIContent(flagSettings.enumName);
		//if (string.IsNullOrEmpty(flagSettings.enumName))
		//	propName = label;

		//EditorGUI.BeginProperty(position, label, property);
		Enum enumNew = EditorGUILayout.EnumFlagsField(label, targetEnum);
		property.intValue = (int)Convert.ChangeType(enumNew, fieldInfo.FieldType);
		//EditorGUI.EndProperty();
    }
}