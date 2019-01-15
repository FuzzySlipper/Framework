using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    [Serializable]
    public class SerializedGameObject : ISerializable {

        [SerializeField] private string _name;
        [SerializeField] private SerializedTransform _tr;
        [SerializeField] private SerializedMetaData _metadata;
        [SerializeField] private List<SerializedGameObject> _children;
        [SerializeField] private List<IUnitySerializable> _components;

        public SerializedGameObject() {}

        public SerializedGameObject(SerializationInfo info, StreamingContext context) {
            _name = (string)info.GetValue("Name", typeof(string));
            _tr = (SerializedTransform)info.GetValue("Tr", typeof(SerializedTransform));
            _metadata = (SerializedMetaData)info.GetValue("Metadata", typeof(SerializedMetaData));
            if (_metadata == null || !_metadata.HasValidSerializedId) {
                _components = (List<IUnitySerializable>) info.GetValue("Components", typeof(List<IUnitySerializable>));
            }
            _children = (List<SerializedGameObject>)info.GetValue("Children", typeof(List<SerializedGameObject>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Name", _name, typeof(string));
            info.AddValue("Tr", _tr, typeof(SerializedTransform));
            info.AddValue("Metadata", _metadata, typeof(SerializedMetaData));
            if (_metadata == null || !_metadata.HasValidSerializedId) {
                info.AddValue("Components", _components, typeof(List<SerializedUnityComponent<Component>>));
            }
            info.AddValue("Children", _children, typeof(List<SerializedGameObject>));
        }

        public SerializedGameObject(GameObject root, PrefabEntity dataHolder) {
            _name = root.name;
            _components = new List<IUnitySerializable>();
            if (dataHolder == null) {
                SetupCreated(root);
            }
            else {
                _metadata = dataHolder.Metadata;
                _metadata.SetDatabaseEntry(dataHolder);
            }
            _metadata.UpdateSerializedId();
            _tr = new SerializedTransform();
            _tr.Set(root.transform);
            _children = new List<SerializedGameObject>();
            foreach (Transform child in root.transform) {
                if (!child.gameObject.activeSelf) {
                    continue;
                }
                var entity = child.GetComponent<PrefabEntity>();
                if (entity == null) {
                    continue;
                }
                _children.Add(new SerializedGameObject(child.gameObject, entity));
            }
            if (Application.isPlaying) {
                var savedData = root.GetComponentsInChildren<ISavedData>();
                for (int i = 0; i < savedData.Length; i++) {
                    savedData[i].SaveMetaData(_metadata);
                }
            }
#if UNITY_EDITOR
            if (Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabType(root) == PrefabType.None) {
                return;
            }
            var mods = UnityEditor.PrefabUtility.GetPropertyModifications(root);
            if (mods == null) {
                return;
            }
            for (int i = 0; i < mods.Length; i++) {
                var component = mods[i].target as Component;
                if (component == null || component is Transform || ReflectionHelper.HasIgnoredAttribute(component.GetType()) || 
                    Serializer.RestrictedTypes.Contains(component.GetType())) {
                    continue;
                }
                if (mods[i].objectReference != null) {
                    _metadata.ComponentDiff.SetDiff(component, mods[i].propertyPath, mods[i].objectReference);
                }
                else {
                    _metadata.ComponentDiff.SetDiff(component, mods[i].propertyPath, mods[i].value);
                    
                }
            }
#endif
        }

        private void SetupCreated(GameObject root) {
            var wc = root.AddComponent<PrefabEntity>();
            wc.SetStatic();
            _metadata = wc.Metadata;
            _metadata.AssetType = AssetType.Scene;
            foreach (var c in root.GetComponents<Component>()) {
                if (c == null ||
                    c is Transform ||
                    c.GetType().GetCustomAttributes(true).Any(x => x is IgnoreFileSerialization)) {
                    continue;
                }
                _components.Add(Serializer.CreateSerializableObject(c));
            }
        }

        public GameObject ToGameObject(Transform parent) {
            GameObject go = null;
            PrefabEntity mc = null;
            switch (_metadata.AssetType) {
                case AssetType.Prefab:
                    mc = ItemPool.Spawn(_metadata.PrefabPath);
                    if (mc != null) {
                        go = mc.gameObject;
                        //Metadata.ComponentDiff.ApplyPatch(go);
                    }
                    break;

            }
            if (go == null) {
                go = new GameObject();
                mc = go.GetOrAddComponent<PrefabEntity>();
                AddComponentsToNewGo(go);
            }
            mc.Metadata = _metadata;
            go.name = _name;
            if (parent != null) {
                go.transform.SetParent(parent);
            }
            _tr.Restore(go.transform);
            foreach (var childNode in _children) {
                var child = childNode.ToGameObject(go.transform);
                child.transform.SetParent(go.transform);
            }
            return go;
        }

        public T ToGameObject<T>(Transform parent) where T : MonoBehaviour {
            return ToGameObject(parent).GetComponent<T>();
        }

        private void AddComponentsToNewGo(GameObject go) {
            if (_components == null) {
                return;
            }
            for (var i = 0; i < _components.Count; i++) {
                var serializedObject = _components[i];
                if (!typeof(Component).IsAssignableFrom(serializedObject.Type)) {
                    Debug.LogError(serializedObject.Type + " does not inherit UnityEngine.Component!");
                    continue;
                }
                var c = go.AddComponent(serializedObject.Type);
                serializedObject.ApplyProperties(c);
            }
        }
    }
}
