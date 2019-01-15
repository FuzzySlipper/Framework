//using System;
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEditor;

//namespace PixelComrades {
//    [CustomPropertyDrawer(typeof(ActorModConfig), true)]
//    public class ActorModDrawer : PropertyDrawer {

//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//            DrawDrawer(property, label, fieldInfo);
//        }
        
//        public static void DrawDrawer(SerializedProperty property, GUIContent label, FieldInfo fieldInfo) {
//            var propertyInstance = property.GetTargetObjectOfProperty() as ActorModConfig;
//            if (propertyInstance == null) {
//                EditorGUILayout.LabelField("null property");
//            }
//            EditorGUILayout.PrefixLabel(label);
//            var typePropery = property.FindPropertyRelative("ModType");
//            var newEnum = (ActorModTypes) EditorGUILayout.EnumPopup((ActorModTypes) typePropery.enumValueIndex);
//            if ((int) newEnum != typePropery.enumValueIndex) {
//                typePropery.enumValueIndex = (int) newEnum;
//                property.serializedObject.ApplyModifiedProperties();
//                if (propertyInstance != null) {
//                    propertyInstance.ConfigureEditorValues(newEnum);
//                }
//                property.serializedObject.Update();
//            }
//            EditorGUILayout.BeginHorizontal();
//            var lbl = EditorGUIUtility.labelWidth;
//            EditorGUIUtility.labelWidth = 75;
//            var turnLength = property.FindPropertyRelative("TurnLength");
//            EditorGUILayout.PropertyField(turnLength);
//            var magMinProp = property.FindPropertyRelative("MagMin");
//            var magMaxProp = property.FindPropertyRelative("MagMax");
//            var min = magMinProp.intValue;
//            var max = magMaxProp.intValue;
//            min = EditorGUILayout.IntField(min);
//            EditorGUIUtility.labelWidth = 2;
//            EditorGUILayout.LabelField("-");
//            max = EditorGUILayout.IntField(max);
//            float fMin = min;
//            float fMax = max;
//            EditorGUILayout.EndHorizontal();
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.MinMaxSlider(ref fMin, ref fMax, 0, 100);
//            EditorGUILayout.EndHorizontal();
//            magMinProp.intValue = (int) fMin;
//            magMaxProp.intValue = (int) fMax;
//            EditorGUIUtility.labelWidth = lbl;
//            if (propertyInstance != null && propertyInstance.EditorMod != null) {
//                propertyInstance.EditorMod.EditorGui(property);
//            }
//            //EditorGUILayout.BeginHorizontal();
//            //var valueArray = property.FindPropertyRelative("Values");
//            //string[] labels = null;
//            //GenericValue[] elements = null;
//            //if (propertyInstance != null && propertyInstance.EditorMod != null) {
//            //    labels = propertyInstance.EditorMod.ValueLabels;
//            //    elements = propertyInstance.EditorMod.GenericValues;
//            //}
//            //int cnt = 0;
//            //for (int i = 0; i < valueArray.arraySize; i++) {
//            //    var value = valueArray.GetArrayElementAtIndex(i);
//            //    if (value == null) {
//            //        continue;
//            //    }
//            //    var elementType = elements != null ? elements[i] : GenericValue.Int;
//            //    var valueLabel = labels != null ? labels[i] : "null";
//            //    value.intValue = elementType.GuilLayout(valueLabel, value.intValue);
//            //    cnt++;
//            //    if (cnt > 2) {
//            //        EditorGUILayout.EndHorizontal();
//            //        EditorGUILayout.BeginHorizontal();
//            //        cnt = 0;
//            //    }
//            //}
//            //EditorGUILayout.EndHorizontal();
//            //EditorGUIUtility.fieldWidth = position.width;
//        }

//    }
//}
