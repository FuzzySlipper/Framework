using UnityEngine;
using System.Collections;
using PixelComrades;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomPropertyDrawer(typeof(Point3))]
public class Point3Drawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty x = property.FindPropertyRelative("x");
        SerializedProperty y = property.FindPropertyRelative("y");
        SerializedProperty z = property.FindPropertyRelative("z");
        float allControls = position.width / 4;
        float labelWidth = allControls / 3;
        float currentWidth = 0;
        EditorGUI.LabelField(new Rect(position.x + currentWidth, position.y, labelWidth, position.height), "X");
        currentWidth += labelWidth;
        x.intValue = EditorGUI.IntField(new Rect(position.x + currentWidth, position.y, allControls, position.height),
            x.intValue);
        currentWidth += allControls;
        EditorGUI.LabelField(new Rect(position.x + currentWidth, position.y, labelWidth, position.height), "Y");
        currentWidth += labelWidth;
        y.intValue = EditorGUI.IntField(new Rect(position.x + currentWidth, position.y, allControls, position.height),
            y.intValue);
        currentWidth += allControls;
        EditorGUI.LabelField(new Rect(position.x + currentWidth, position.y, labelWidth, position.height), "Z");
        currentWidth += labelWidth;
        z.intValue = EditorGUI.IntField(new Rect(position.x + currentWidth, position.y, allControls, position.height),
            z.intValue);
    }
}