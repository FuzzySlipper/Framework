using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer (typeof (MinMaxSliderIntAttribute))]
class MinMaxIntSliderDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) + 16;
    }
    
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        var minValue = property.FindPropertyRelative("Min");
        var maxValue = property.FindPropertyRelative("Max");
	    var min = minValue.intValue;
	    var max = maxValue.intValue;
        var attr = attribute as MinMaxSliderIntAttribute;
        EditorGUI.BeginChangeCheck();
        var xDivision = position.width * 0.33f;
        var yDivision = position.height * 0.5f;
        EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, yDivision), label);
        EditorGUI.LabelField(new Rect(position.x, position.y + yDivision, position.width, yDivision), min.ToString("0.##"));
        EditorGUI.LabelField(new Rect(position.x + position.width - 40f, position.y + yDivision, position.width, yDivision), max.ToString("0.##"));
        var minFloat = (float) min;
        var maxFloat = (float) max;
        if (maxFloat < minFloat) {
            maxFloat = minFloat;
        }
        EditorGUI.MinMaxSlider(new Rect(position.x + 38f, position.y + yDivision, position.width - 80f, yDivision), ref minFloat, ref maxFloat, attr.min, attr.max);
        min = (int) minFloat;
        max = (int) maxFloat;
        EditorGUI.LabelField(new Rect(position.x + xDivision, position.y, xDivision, yDivision), "Min: ");
        min = Mathf.Clamp(EditorGUI.IntField(new Rect(position.x + xDivision + 30, position.y, xDivision - 30, yDivision), min), attr.min, attr.max);
        EditorGUI.LabelField(new Rect(position.x + xDivision * 2f, position.y, xDivision, yDivision) , "Max: ");
        max = Mathf.Clamp(EditorGUI.IntField(new Rect(position.x + xDivision * 2f + 34, position.y, xDivision - 24, yDivision), max), attr.min, attr.max);
        if (EditorGUI.EndChangeCheck()) {
            minValue.intValue = min;
            maxValue.intValue = max;
        }
    }
}
