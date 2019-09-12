using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class CachedGenericComponent<T> : ISerializable {

        private int _serializedId = -1;
        private string _transformChild;
        private int _entityId = -1;
        private System.Type _componentType;
        
        private T _component;
        
        public T Component {
            get {
                if (_component != null) {
                    return _component;
                }
                TryRestore();
                return _component;
            }
        }

        private void TryRestore() {
            if (_serializedId >= 0) {
                var prefab = Serializer.GetPrefabEntity(_serializedId);
                if (prefab != null) {
                    if (!string.IsNullOrEmpty(_transformChild)) {
                        var targetTr = prefab.transform.Find(_transformChild);
                        if (targetTr != null) {
                            _component = targetTr.GetComponent<T>();
                        }
                    }
                    if (_component == null) {
                        _component = prefab.GetComponentInChildren<T>();
                    }
                }
                return;
            }
            if (_entityId < 0) {
                return;
            }
            var entity = EntityController.Get(_entityId);
            if (entity == null) {
                return;
            }
            var cref = entity.GetComponentReference(_componentType);
            if (cref != null) {
                _component = (T) cref.Value.Get();
            }
        }

        public void Clear() {
            _serializedId = -1;
            _transformChild = null;
            _entityId = -1;
        }
        
        public CachedGenericComponent(T component) {
            Set(component);
        }

        public void Set(T component) {
            _component = component;
            if (_component == null) {
                Clear();
                return;
            }
            if (component is UnityEngine.Component unityComponent) {
                var prefab = PrefabEntity.FindPrefabRoot(unityComponent.transform);
                if (prefab != null) {
                    _serializedId = prefab.PrefabId;
                    _transformChild = unityComponent.transform.GetPath();
                }
                else {
                    Debug.LogErrorFormat("{0}: unable to find prefab root on {1}", component.GetType(), unityComponent.gameObject.name);
                }
                return;
            }
            if (component is IComponent ecsComponent) {
                _entityId = ecsComponent.GetEntity();
                _componentType = component.GetType();
                return;
            }
            Debug.LogErrorFormat("{0} is not a valid type of animator", component.GetType());
        }
        
        public CachedGenericComponent(SerializationInfo info, StreamingContext context) {
            _serializedId = info.GetValue(nameof(_serializedId), _serializedId);
            _transformChild = info.GetValue(nameof(_transformChild), _transformChild);
            _entityId = info.GetValue(nameof(_entityId), _entityId);
            _componentType = ((SerializableType) info.GetValue(nameof(_componentType), typeof(SerializableType))).TargetType;
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_serializedId), _serializedId);
            info.AddValue(nameof(_transformChild), _transformChild);
            info.AddValue(nameof(_entityId), _entityId);
            info.AddValue(nameof(_componentType), new SerializableType(_componentType));
        }
    }
}
