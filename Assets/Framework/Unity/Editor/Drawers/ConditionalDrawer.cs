using System;
using System.Collections.Generic;
using System.Reflection;
using PixelComrades;
using UnityEditor;
using UnityEngine;

//Decides if a varible should be shown in the inspector depending on another (boolean) varible
[CustomPropertyDrawer(typeof(ConditionalAttribute))]
public class ConditionalDrawer : PropertyDrawer {

    private ConditionalAttribute attr {
        get { return (ConditionalAttribute) attribute; }
    }

    private static string GetPath(SerializedProperty property) {
        var path = property.propertyPath;
        var index = path.LastIndexOf(".");
        return path.Substring(0, index + 1);
    }

    private static bool Check(object propertyValue, ConditionalAttribute.Comparison comparison, object value) {
        if (!(propertyValue is IComparable) || !(value is IComparable))
            throw new Exception("Check using non basic type");

        //if (propertyValue.GetType() != value.GetType())
        //  throw new Exception("Type missmatch");


        switch (comparison) {
            case ConditionalAttribute.Comparison.Equals:
                return ((IComparable) propertyValue).CompareTo(value) == 0;

            case ConditionalAttribute.Comparison.NotEqualTo:
                return ((IComparable) propertyValue).CompareTo(value) != 0;

            case ConditionalAttribute.Comparison.EqualsOrGreaterThan:
                return ((IComparable) propertyValue).CompareTo(value) >= 0;

            case ConditionalAttribute.Comparison.EqualsOrLessThan:
                return ((IComparable) propertyValue).CompareTo(value) <= 0;

            case ConditionalAttribute.Comparison.GreaterThan:
                return ((IComparable) propertyValue).CompareTo(value) > 0;

            case ConditionalAttribute.Comparison.LessThan:
                return ((IComparable) propertyValue).CompareTo(value) < 0;

            default:
                break;
        }
        return false;
    }

    private static bool ShouldShow(SerializedProperty property, ConditionalAttribute.Where[] conditions) {
        if (conditions == null || conditions.Length == 0) return true;

        var path = GetPath(property);

        var previous = false;
        for (var i = 0; i < conditions.Length; i++) {
            //Find SerializedProperty used for this condition
            var conditionalProperty = property.serializedObject.FindProperty(path + conditions[i].propertyName);
            if (conditionalProperty == null) {
                throw new Exception("Method " + path + conditions[i].propertyName +
                                    " does not exist. \n");
            }
            object propertyValue;
            if (conditionalProperty.propertyType == SerializedPropertyType.Integer)
                propertyValue = conditionalProperty.intValue;
            else if (conditionalProperty.propertyType == SerializedPropertyType.Float)
                propertyValue = conditionalProperty.floatValue;
            else if (conditionalProperty.propertyType == SerializedPropertyType.Boolean)
                propertyValue = conditionalProperty.boolValue;
            else if (conditionalProperty.propertyType == SerializedPropertyType.Enum)
                if (conditions[i].value.GetType().IsEnum)
                    propertyValue = Enum.Parse(conditions[i].value.GetType(),
                        conditionalProperty.enumNames[conditionalProperty.enumValueIndex]);
                else if (conditions[i].value.GetType() == typeof(string))
                    propertyValue = conditionalProperty.enumNames[conditionalProperty.enumValueIndex];
                else propertyValue = conditionalProperty.enumValueIndex;
            else
                throw new Exception("Type " + conditionalProperty.propertyType +
                                    " needs implementing in ConditionalDrawer.ShouldShow.\n");

            var test = Check(propertyValue, conditions[i].comparison, conditions[i].value);

            if (i != 0)
                switch (conditions[i - 1].logical) {
                    case ConditionalAttribute.Logical.AND:
                        test = test && previous;
                        break;
                    case ConditionalAttribute.Logical.OR:
                        test = test || previous;
                        break;
                }

            previous = test;
        }
        return previous;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (!attr.shouldBeShown) {
            return -2f;
        }
        //return EditorGUI.GetPropertyHeight(property, label, true);
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var conditionalAttributes = new List<ConditionalAttribute>();
        foreach (var atr in fieldInfo.GetCustomAttributes(false)) {
            if (atr is ConditionalAttribute) {
                conditionalAttributes.Add((ConditionalAttribute) atr);
            }
        }

        var conditionCount = 0;
        for (var a = 0; a < conditionalAttributes.Count; a++)
            if (conditionalAttributes[a].conditions != null)
                conditionCount += conditionalAttributes[a].conditions.Length;

        var conditions = new ConditionalAttribute.Where[conditionCount];
        var index = 0;
        for (var a = 0; a < conditionalAttributes.Count; a++)
            if (conditionalAttributes[a].conditions != null)
                for (var c = 0; c < conditionalAttributes[a].conditions.Length; c++)
                    conditions[index++] = conditionalAttributes[a].conditions[c];

        attr.shouldBeShown = true;
        try {
            attr.shouldBeShown = ShouldShow(property, conditions);
        }
        catch (Exception e) {
            Debug.LogError(e.ToString());
            Debug.LogError(e.StackTrace.ToString());
        }

        if (attr.shouldBeShown) {
            DisplayCustom(position, property, label, fieldInfo);
        }
    }

    public static void DisplayCustom(Rect position, SerializedProperty property, GUIContent label, FieldInfo fieldInfo) {
        foreach (var atr in fieldInfo.GetCustomAttributes(false)) {
            if (atr is RangeAttribute) {
                var range = (RangeAttribute) atr;
                property.floatValue = EditorGUI.Slider(position, label, property.floatValue, range.min, range.max);
                return;
            }
        }
        EditorGUI.PropertyField(position, property, label, true);
    }
}