using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace PixelComrades {
//    [CustomPropertyDrawer(typeof(SpriteAnimationReference))]
//    public class SpriteAnimationReferenceDrawer : PropertyDrawer {
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//            var reference = property.FindPropertyRelative(nameof(SpriteAnimationReference.Reference));
//            EditorGUI.PropertyField(position, reference, label, true);
//            EditorGUI.EndProperty();
//        }
//    }
    
    //[CustomPropertyDrawer(typeof(PossibleAbility))]
    //public class PossibleAbilityDrawer : PropertyDrawer {
    //    private const float MinPercent = 0;
    //    private const float MaxPercent = 100;
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        //label = EditorGUI.BeginProperty(position, label, property);
    //        //Rect contentPosition = EditorGUI.PrefixLabel(position, label);
    //        //EditorGUI.indentLevel = 0;
    //        //contentPosition.height *= 0.5f;
    //        //SerializedProperty percent = property.FindPropertyRelative("Chance");
    //        //if (percent != null) {
    //        //    percent.floatValue = (float) System.Math.Round((decimal)
    //        //        EditorGUI.Slider(contentPosition, percent.floatValue, MinPercent, MaxPercent), 2);
    //        //}
    //        ////contentPosition.width *= 0.5f;
    //        //contentPosition.y += contentPosition.height;
    //        //SerializedProperty ability = property.FindPropertyRelative("Ability");
    //        //EditorGUI.ObjectField(contentPosition, ability, typeof(AbilityItem));
    //        //EditorGUI.EndProperty();
    //        EditorGUILayout.BeginVertical();
    //        EditorGUILayout.BeginHorizontal();
    //        SerializedProperty percent = property.FindPropertyRelative("Chance");
    //        if (percent != null) {
    //            percent.floatValue = (float) System.Math.Round((decimal)
    //                EditorGUILayout.Slider(percent.floatValue, MinPercent, MaxPercent), 2);
    //        }
    //        var go = property.serializedObject.targetObject as MonoBehaviour;
    //        SerializedProperty ability = property.FindPropertyRelative("Ability");
    //        if (go != null) {
    //            EditorGUILayout.ObjectField(ability, typeof(Ability), GUIContent.none);
    //            //if (GUILayout.Button(ability.displayName)) {
    //            var options = go.GetComponentsInChildren<Ability>();
    //            List<string> labels = new List<string>();
    //            int curr = 0;
    //            labels.Add("None");
    //            var currAbility = ability.objectReferenceValue as Ability;
    //            //var currAbility = PropertyDrawerUtility.GetActualObjectForSerializedProperty<PossibleAbility>(fieldInfo, property);
    //            for (int i = 0; i < options.Length; i++) {
    //                labels.Add(string.Format("{0}:{1}", options[i].GetType().Name, options[i].name));
    //                if (currAbility != null && options[i] == currAbility) {
    //                    curr = i + 1;
    //                }
    //            }
    //            var newIndex = EditorGUILayout.Popup(curr, labels.ToArray());
    //            if (newIndex != curr) {
    //                if (newIndex == 0) {
    //                    ability.objectReferenceValue = null;
    //                }
    //                else {
    //                    ability.objectReferenceValue = options[newIndex - 1];
    //                }
    //            }
    //        }
    //        else {
    //            EditorGUILayout.LabelField(property.serializedObject.targetObject.GetType().ToString());
    //            EditorGUILayout.ObjectField(ability, typeof(Ability), GUIContent.none);
    //        }
            
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.EndVertical();
    //        //EditorGUI.EndProperty();
    //    }
    //    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    //    //    return base.GetPropertyHeight(property, label) * 2f + 5f;
    //    //}
    //}

    //[CustomPropertyDrawer(typeof(MinMaxScalingValueHolder), true)]
    //public class BaseValueMinHolderDrawer : PropertyDrawer {

    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        EditorGUILayout.BeginVertical();
    //        EditorGUILayout.BeginHorizontal();
    //        var scaling = property.FindPropertyRelative("_statScaling");
    //        var min = property.FindPropertyRelative("_baseMin");
    //        var max = property.FindPropertyRelative("_baseMax");
    //        min.floatValue = (float) System.Math.Round((decimal)
    //                EditorGUILayout.FloatField(min.floatValue), 2);
    //        max.floatValue = (float) System.Math.Round((decimal)
    //                EditorGUILayout.FloatField(max.floatValue), 2);
    //        EditorGUILayout.PropertyField(scaling);
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.BeginHorizontal();
    //        SerializedProperty statType = property.FindPropertyRelative("StatType");
    //        SerializedProperty statIndex = property.FindPropertyRelative("StatIndex");
    //        EditorGUILayout.PropertyField(statType, GUIContent.none);
    //        StatTypes stat = (StatTypes)statType.enumValueIndex;
    //        statIndex.intValue = stat.GuilLayout("", statIndex.intValue);
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.EndVertical();
    //    }
    //}


    //[CustomPropertyDrawer(typeof(ValueWithScalingHolder), true)]
    //public class BaseValueHolderDrawer : PropertyDrawer {

    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        EditorGUILayout.BeginVertical();
    //        EditorGUILayout.BeginHorizontal();
    //        var percent = property.FindPropertyRelative("_statScaling");
    //        var bonus = property.FindPropertyRelative("_baseValue");
    //        if (bonus != null) {
    //            bonus.floatValue = (float) System.Math.Round((decimal)
    //                EditorGUILayout.FloatField(bonus.floatValue), 2);
    //        }
    //        EditorGUILayout.PropertyField(percent);
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.BeginHorizontal();
    //        SerializedProperty statType = property.FindPropertyRelative("StatType");
    //        SerializedProperty statIndex = property.FindPropertyRelative("StatIndex");
    //        EditorGUILayout.PropertyField(statType, GUIContent.none);
    //        StatTypes stat = (StatTypes)statType.enumValueIndex;
    //        statIndex.intValue = stat.GuilLayout("", statIndex.intValue);
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.EndVertical();
    //    }
    //}

    //[CustomPropertyDrawer(typeof(GenericStatHolder), true)]
    //public class GenericStatHolderDrawer : PropertyDrawer {
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        label = EditorGUI.BeginProperty(position, label, property);
    //        Rect contentPosition = EditorGUI.PrefixLabel(position, label);
    //        EditorGUI.indentLevel = 0;
    //        contentPosition.height *= 0.5f;
    //        SerializedProperty percent = property.FindPropertyRelative("_statScaling");
    //        if (percent != null) {
    //            //percent.floatValue = (float) System.Math.Round((decimal)
    //            //    EditorGUI.Slider(contentPosition, percent.floatValue, MinPercent, MaxPercent), 2);
    //            EditorGUI.PropertyField(contentPosition, percent);
    //        }
    //        else {
    //            var bonus = property.FindPropertyRelative("_bonus");
    //            if (bonus != null) {
    //                bonus.floatValue = (float)System.Math.Round((decimal)
    //                EditorGUI.Slider(contentPosition, bonus.floatValue, 0, 100), 2);
    //            }
    //        }
    //        contentPosition.width *= 0.5f;
    //        //contentPosition.y += contentPosition.height;
    //        SerializedProperty statType = property.FindPropertyRelative("StatType");
    //        SerializedProperty statIndex = property.FindPropertyRelative("StatIndex");
    //        EditorGUI.PropertyField(contentPosition, statType, GUIContent.none);
    //        contentPosition.x += contentPosition.width;
    //        StatTypes stat = (StatTypes)statType.enumValueIndex;
    //        statIndex.intValue = stat.GuilLayout(contentPosition, statIndex.intValue);
    //        EditorGUI.EndProperty();
    //    }

    //    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    //    //    return base.GetPropertyHeight(property, label) * 2f + 5f;
    //    //}
    //}
    //[CustomPropertyDrawer(typeof(GenericStatHolder), true)]
    //public class GenericStatHolderDrawer : PropertyDrawer {
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        SerializedProperty percent = property.FindPropertyRelative("_statScaling");
    //        EditorGUILayout.BeginVertical();
    //        if (percent != null) {
    //            //percent.floatValue = (float) System.Math.Round((decimal)
    //            //    EditorGUI.Slider(contentPosition, percent.floatValue, MinPercent, MaxPercent), 2);
    //            EditorGUILayout.PropertyField(percent);
    //        }
    //        else {
    //            var bonus = property.FindPropertyRelative("_bonus");
    //            if (bonus != null) {
    //                bonus.floatValue = (float)System.Math.Round((decimal)
    //                EditorGUILayout.Slider(bonus.floatValue, 0, 100), 2);
    //            }
    //        }
    //        EditorGUILayout.BeginHorizontal();
    //        SerializedProperty statType = property.FindPropertyRelative("StatType");
    //        SerializedProperty statIndex = property.FindPropertyRelative("StatIndex");
    //        EditorGUILayout.PropertyField(statType, GUIContent.none);
    //        StatTypes stat = (StatTypes)statType.enumValueIndex;
    //        statIndex.intValue = stat.GuilLayout("", statIndex.intValue);
    //        EditorGUILayout.EndHorizontal();
    //        EditorGUILayout.EndVertical();
    //    }

    //    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    //    //    return base.GetPropertyHeight(property, label) * 2f + 5f;
    //    //}
    //}

    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer {
        private const float MinPercent = 0;
        private const float MaxPercent = 100;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            contentPosition.height *= 0.5f;
            contentPosition.width *= 0.5f;
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");
            min.floatValue = EditorGUI.FloatField(contentPosition, min.floatValue);
            contentPosition.x += contentPosition.width;
            max.floatValue = EditorGUI.FloatField(contentPosition, max.floatValue);
            contentPosition.x -= contentPosition.width;
            contentPosition.width *= 2f;
            contentPosition.y += contentPosition.height;
            float minfloat = (float)System.Math.Round((decimal)min.floatValue);
            float maxfloat = (float)System.Math.Round((decimal)max.floatValue);
            EditorGUI.MinMaxSlider(contentPosition, ref minfloat, ref maxfloat, MinPercent, MaxPercent);
            min.floatValue = minfloat;
            max.floatValue = maxfloat;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 2f + 5f;
        }
    }

    [CustomPropertyDrawer(typeof(NormalizedFloatRange))]
    public class NormalizedFloatRangeDrawer : PropertyDrawer {
        private const float MinPercent = 0;
        private const float MaxPercent = 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            EditorGUI.indentLevel = 0;
            contentPosition.height *= 0.5f;
            contentPosition.width *= 0.5f;
            SerializedProperty min = property.FindPropertyRelative("Min");
            SerializedProperty max = property.FindPropertyRelative("Max");
            min.floatValue = EditorGUI.FloatField(contentPosition, min.floatValue);
            contentPosition.x += contentPosition.width;
            max.floatValue = EditorGUI.FloatField(contentPosition, max.floatValue);
            contentPosition.x -= contentPosition.width;
            contentPosition.width *= 2f;
            contentPosition.y += contentPosition.height;
            float minfloat = min.floatValue;
            float maxfloat = max.floatValue;
            EditorGUI.MinMaxSlider(contentPosition, ref minfloat, ref maxfloat, MinPercent, MaxPercent);
            min.floatValue = minfloat;
            max.floatValue = maxfloat;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 2f + 5f;
        }
    }

    [CustomPropertyDrawer(typeof(TargetAnimator), true)]
    public class TargetAnimatorDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            TargetAnimator animator = PropertyDrawerUtility.GetActualObjectForSerializedProperty<TargetAnimator>(fieldInfo, property);
            if (animator != null) {
                var contentPosition = EditorGUI.PrefixLabel(position, label);
                var width = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth *= 0.5f;
                EditorGUI.PropertyField(contentPosition, property, new GUIContent(animator.Description));
                EditorGUIUtility.labelWidth = width;
            }
            else {
                EditorGUI.PropertyField(position, property, label);
            }
            //EditorGUI.PropertyField(position, property, animator != null ? new GUIContent(animator.Description) : label);
            EditorGUI.EndProperty();
        }

        //protected override void DrawPropertyRect(Rect position, IPropertyValueEntry<T> entry, GUIContent label) {

        //    //var property = entry.SmartValue[ as SerializedProperty;
        //    //label = EditorGUI.BeginProperty(position, label, property);

        //    //TargetAnimator animator = PropertyDrawerUtility.GetActualObjectForSerializedProperty<TargetAnimator>(fieldInfo, property);
        //    var animator = entry.SmartValue;
        //    if (animator != null) {
        //        //var contentPosition = EditorGUI.PrefixLabel(position, label);
        //        var width = EditorGUIUtility.labelWidth;
        //        EditorGUIUtility.labelWidth *= 0.5f;
        //        InspectorUtilities.DrawProperty(entry.Property, new GUIContent(animator.Description));
        //        //EditorGUI.PropertyField(contentPosition, property, new GUIContent(animator.Description));
        //        EditorGUIUtility.labelWidth = width;
        //    }
        //    else {
        //        InspectorUtilities.DrawProperty(entry.Property, label);
        //        //EditorGUI.PropertyField(position, property, label);
        //    }
        //    //EditorGUI.PropertyField(position, property, animator != null ? new GUIContent(animator.Description) : label);
        //    EditorGUI.EndProperty();
        //    base.DrawPropertyRect(position, entry, label);
        //}
    }

    //[CustomPropertyDrawer(typeof(ImpactHolder), true)]
    //public class ImpactDrawer : PropertyDrawer {
    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    //        label = EditorGUI.BeginProperty(position, label, property);
    //        ImpactHolder target = PropertyDrawerUtility.GetActualObjectForSerializedProperty<ImpactHolder>(fieldInfo, property);
    //        if (target != null) {
    //            var contentPosition = EditorGUI.PrefixLabel(position, label);
    //            var width = EditorGUIUtility.labelWidth;
    //            EditorGUIUtility.labelWidth *= 0.5f;
    //            EditorGUI.PropertyField(contentPosition, property, new GUIContent(target.Description.ToString()));
    //            EditorGUIUtility.labelWidth = width;
    //        }
    //        else {
    //            EditorGUI.PropertyField(position, property, label);
    //        }
    //        //EditorGUI.PropertyField(position, property, animator != null ? new GUIContent(animator.Description) : label);
    //        EditorGUI.EndProperty();
    //    }
    //}

    public class PropertyDrawerUtility {
        public static T GetActualObjectForSerializedProperty<T>(FieldInfo fieldInfo, SerializedProperty property) where T : class {
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (obj == null) { return null; }

            T actualObject = null;
            if (obj.GetType().IsArray) {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                if (index < ((T[])obj).Length) {
                    actualObject = ((T[])obj)[index];
                }
            }
            else {
                actualObject = obj as T;
            }
            return actualObject;
        }
    }
    /// <summary>
    /// Extension class for SerializedProperties
    /// See also: http://answers.unity3d.com/questions/627090/convert-serializedproperty-to-custom-class.html
    /// </summary>
    public static class SerializedPropertyExtensions 
     {
        public static object GetTargetObjectOfProperty(this SerializedProperty prop) {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements) {
                if (element.Contains("[")) {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Imp(object source, string name, int index) {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++) {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        private static object GetValue_Imp(object source, string name) {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null) {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Get the object the serialized property holds by using reflection
        /// </summary>
        /// <typeparam name="T">The object type that the property contains</typeparam>
        /// <param name="property"></param>
        /// <returns>Returns the object type T if it is the type the property actually contains</returns>
        public static T GetValue<T>(this SerializedProperty property)
         {
             return GetNestedObject<T>(property.propertyPath, GetSerializedPropertyRootComponent(property));
         }
 
         /// <summary>
         /// Set the value of a field of the property with the type T
         /// </summary>
         /// <typeparam name="T">The type of the field that is set</typeparam>
         /// <param name="property">The serialized property that should be set</param>
         /// <param name="value">The new value for the specified property</param>
         /// <returns>Returns if the operation was successful or failed</returns>
         public static bool SetValue<T>(this SerializedProperty property, T value)
         {
             
             object obj = GetSerializedPropertyRootComponent(property);
             //Iterate to parent object of the value, necessary if it is a nested object
             string[] fieldStructure = property.propertyPath.Split('.');
             for (int i = 0; i < fieldStructure.Length - 1; i++)
             {
                 obj = GetFieldOrPropertyValue<object>(fieldStructure[i], obj);
             }
             string fieldName = fieldStructure.Last();
 
             return SetFieldOrPropertyValue(fieldName, obj, value);
             
         }
 
         /// <summary>
         /// Get the component of a serialized property
         /// </summary>
         /// <param name="property">The property that is part of the component</param>
         /// <returns>The root component of the property</returns>
         public static Component GetSerializedPropertyRootComponent(SerializedProperty property)
         {
             return (Component)property.serializedObject.targetObject;
         }
 
         /// <summary>
         /// Iterates through objects to handle objects that are nested in the root object
         /// </summary>
         /// <typeparam name="T">The type of the nested object</typeparam>
         /// <param name="path">Path to the object through other properties e.g. PlayerInformation.Health</param>
         /// <param name="obj">The root object from which this path leads to the property</param>
         /// <param name="includeAllBases">Include base classes and interfaces as well</param>
         /// <returns>Returns the nested object casted to the type T</returns>
         public static T GetNestedObject<T>(string path, object obj, bool includeAllBases = false)
         {
             foreach (string part in path.Split('.'))
             {
                 obj = GetFieldOrPropertyValue<object>(part, obj, includeAllBases);
             }
             return (T)obj;
         }
 
         public static T GetFieldOrPropertyValue<T>(string fieldName, object obj, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
         {
             FieldInfo field = obj.GetType().GetField(fieldName, bindings);
             if (field != null) return (T)field.GetValue(obj);
 
             PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
             if (property != null) return (T)property.GetValue(obj, null);
 
             if (includeAllBases)
             {
 
                 foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                 {
                     field = type.GetField(fieldName, bindings);
                     if (field != null) return (T)field.GetValue(obj);
 
                     property = type.GetProperty(fieldName, bindings);
                     if (property != null) return (T)property.GetValue(obj, null);
                 }
             }
 
             return default(T);
         }
 
         public static bool SetFieldOrPropertyValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
         {
             FieldInfo field = obj.GetType().GetField(fieldName, bindings);
             if (field != null)
             {
                 field.SetValue(obj, value);
                 return true;
             }
 
             PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);
             if (property != null)
             {
                 property.SetValue(obj, value, null);
                 return true;
             }
 
             if (includeAllBases)
             {
                 foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                 {
                     field = type.GetField(fieldName, bindings);
                     if (field != null)
                     {
                         field.SetValue(obj, value);
                         return true;
                     }
 
                     property = type.GetProperty(fieldName, bindings);
                     if (property != null)
                     {
                         property.SetValue(obj, value, null);
                         return true;
                     }
                 }
             }
             return false;
         }
 
         public static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type, bool includeSelf = false)
         {
             List<Type> allTypes = new List<Type>();
 
             if (includeSelf) allTypes.Add(type);
 
             if (type.BaseType == typeof(object))
             {
                 allTypes.AddRange(type.GetInterfaces());
             }
             else {
                 allTypes.AddRange(
                         Enumerable
                         .Repeat(type.BaseType, 1)
                         .Concat(type.GetInterfaces())
                         .Concat(type.BaseType.GetBaseClassesAndInterfaces())
                         .Distinct());
             }
 
             return allTypes;
         }
     }
}