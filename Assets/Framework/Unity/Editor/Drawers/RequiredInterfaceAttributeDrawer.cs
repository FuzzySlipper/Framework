using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))] 
    public class RequireInterfaceAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var propType = property.propertyType;
            if (propType != SerializedPropertyType.ObjectReference) {
                Debug.LogError("Can only be used on fields of type UnityEngine.Object or one of its subclasses!");
                return;
            }
            var propertyObject = property.objectReferenceValue;
            if (propertyObject == null) {
                return;
            }
            var a = (RequireInterfaceAttribute) attribute;
            var allowedType = a.TargetType;
            //var objType = propertyObject.GetType();
            //var interfaces = objType.GetInterfaces();
            //var interfaceType = interfaces.FirstOrDefault(x => x == allowedType);
            //if (interfaceType == null) {
            //    Debug.LogError(propertyObject + " does not inherit from interface of type " + allowedType);
            //    property.objectReferenceValue = null;
            //}
            //property.serializedObject.ApplyModifiedProperties();
            
            MonoBehaviour mono = property.serializedObject.targetObject as MonoBehaviour;
            GameObject go = property.serializedObject.targetObject as GameObject;
            if (go != null || mono != null) {
                position.width *= 0.5f;
                if (go == null) {
                    go = mono.gameObject;
                }
            }
            else {
                var scriptable = property.serializedObject.targetObject as ScriptableObject;
                if (scriptable != null) {
                    var type = scriptable.GetType();
                    if (!allowedType.IsAssignableFrom(type)) {
                        Debug.LogError(propertyObject + " does not inherit from interface of type " + allowedType);
                        property.objectReferenceValue = null;
                    }
                }
            }
            EditorGUI.PropertyField(position, property, label);
            if (go != null) {
                var interfaces = go.GetComponents(allowedType);
                if (interfaces == null) {
                    Debug.LogError(propertyObject + " does not contain any interfaces of type " + allowedType);
                    property.objectReferenceValue = null;
                    return;
                }
                List<string> labels = new List<string>();
                int curr = 0;
                labels.Add("None");
                for (int i = 0; i < interfaces.Length; i++) {
                    labels.Add(string.Format("{0}:{1}", interfaces[i].GetType().Name, interfaces[i].name));
                    if (mono != null && interfaces[i] == mono) {
                        curr = i + 1;
                    }
                }
                position.position = new Vector2(position.x + position.width, position.y);
                var newIndex = EditorGUI.Popup(position, curr, labels.ToArray());
                if (newIndex != curr) {
                    if (newIndex == 0) {
                        property.objectReferenceValue = null;
                    }
                    else {
                        property.objectReferenceValue = interfaces[newIndex - 1];
                    }
                }
            }
        }
    }
}