using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    /**
     * Stores a dictionary of modified values and their corresponding component.  This is 
     * used to serialize changes to prefabs without writing the entirity of their serialized
     * data to disk.
     */
    public class SerializedComponentDifferences : ISerializable {

        private List<Type> _keys;
        private List<Dictionary<string, object>> _values;
        private Dictionary<Component, Dictionary<string, object>> _modifiedValues;
        public Dictionary<Component, Dictionary<string, object>> ModifiedValues { get { return _modifiedValues; } }

        public SerializedComponentDifferences() {
            _modifiedValues = new Dictionary<Component, Dictionary<string, object>>();
        }

        public SerializedComponentDifferences(SerializationInfo info, StreamingContext context) {
            _modifiedValues = new Dictionary<Component, Dictionary<string, object>>();
            _keys = (List<Type>)info.GetValue("components", typeof(List<Type>));
            _values = (List<Dictionary<string, object>>)info.GetValue("values", typeof(List<Dictionary<string, object>>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (_modifiedValues == null) {
                _modifiedValues = new Dictionary<Component, Dictionary<string, object>>();
            }
            var keys = _modifiedValues.Keys.Select(x => x.GetType()).ToList();
            info.AddValue("components", keys, typeof(List<Type>));
            info.AddValue("values", _modifiedValues.Values.ToList(), typeof(List<Dictionary<string, object>>));
        }

        /**
         * Add a diff entry for a component.  `component` points to the edited component, name is the variable name, and 
         * value is the new value.
         */
        public static void AddDiff(Component component, string name, object value) {
            var go = component.gameObject;
            var mdComponent = go.GetComponent<PrefabEntity>();
            if (mdComponent == null) {
                mdComponent = go.AddComponent<PrefabEntity>();
            }
            mdComponent.Metadata.ComponentDiff.SetDiff(component, name, value);
        }

        public void SetDiff(Component component, string name, object originalValue) {
            string fieldName = ReflectionHelper.ScrubUnityInternal(name);
            object targetValue = originalValue;
            var targetComponent = originalValue as Component;
            if (targetComponent != null) {
                if (targetComponent.gameObject != component.gameObject) {
                    var targetEntity = targetComponent.gameObject.GetComponent<PrefabEntity>();
                    if (targetEntity == null) {
                        Debug.LogErrorFormat("No entity on {0} for component {1}", targetComponent.gameObject.name, component.name);
                        return;
                    }
                    targetValue = new SerializedComponentReference(targetEntity, targetComponent);
                }
            }
            else {
                var targetGo = originalValue as GameObject;
                if (targetGo != null && targetGo != component.gameObject) {
                    var targetEntity = targetGo.GetComponent<PrefabEntity>();
                    if (targetEntity == null) {
                        Debug.LogErrorFormat("No entity on {0} for component {1}", targetGo.name, component.name);
                        return;
                    }
                    targetValue = new SerializedGameObjectReference(targetEntity);
                }
            }
            Dictionary<string, object> v;
            if (_modifiedValues.TryGetValue(component, out v)) {
                if (v.ContainsKey(fieldName)) {
                    v[fieldName] = targetValue;
                }
                else {
                    v.Add(fieldName, targetValue);
                }
            }
            else {
                _modifiedValues.Add(component, new Dictionary<string, object> { { fieldName, targetValue } });
            }
        }

        /**
         * Called after an object is deserialized.  This interates through components and sets the modified values,
         * while simultaneously rebuilding modifiedValues so that the keys point to actual component objects.
         */
        public void ApplyDifferences(PrefabEntity target) {
            if (_keys == null || _keys.Count < 1) {
                return;
            }
            if (_values.Count != _keys.Count) {
                Debug.LogErrorFormat("Invalid keys count {0} to values count {1}", _keys.Count, _values.Count);
                return;
            }
            _modifiedValues = new Dictionary<Component, Dictionary<string, object>>();
            var limit = _keys.Count;
            // if a component has multiple instances on an object, this will make sure that they remain distinct (probably)
            var dupComponents = new Dictionary<Type, int>();
            for (var i = 0; i < limit; i++) {
                var components = target.GetComponents(_keys[i]);
                if (dupComponents.ContainsKey(_keys[i])) {
                    dupComponents[_keys[i]]++;
                }
                else {
                    dupComponents.Add(_keys[i], 0);
                }
                var index = MathEx.Min(dupComponents[_keys[i]], components.Length - 1);
                if (index >= components.Length || index < 0) {
                    Debug.LogErrorFormat("Index {0} on {1} out of range", index, target.name);
                    continue;
                }
                if (i >= _values.Count) {
                    Debug.LogErrorFormat("Values Index {0} on {1} out of range", i, target.name);
                    continue;
                }
                var component = components[index];
                _modifiedValues.Add(component, _values[i]);
                foreach (var kvp in _values[i]) {
                    var key = kvp.Key;
                    if (!key.Contains("Array")) {
                        ReflectionHelper.SetValue(components[index], key, kvp.Value);
                        continue;
                    }
                    var parts = key.Split('.');
                    key = parts[0];
                    for (int p = 0; p < parts.Length; p++) {
                        if (p < 2) {
                            continue;
                        }
                        var part = parts[p];
                        if (part == "size") {
                            int newSize;
                            if (int.TryParse(kvp.Value as string, out newSize)) {
                                ReflectionHelper.ResizeArray(components[index], key, newSize);
                                break;
                            }
                        }
                        if (part.Contains("data")) {
                            var indexString = part.Substring(5, 1);
                            int arrayIdx;
                            if (int.TryParse(indexString, out arrayIdx)) {
                                ReflectionHelper.SetArrayObj(components[index], key, arrayIdx, kvp.Value);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}