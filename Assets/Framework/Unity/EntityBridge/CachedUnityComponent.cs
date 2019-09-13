using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class CachedUnityComponent<T> : ISerializable, IDisposable where T : UnityEngine.Component {

        private int _serializedId = -1;
        private string _transformChild;
        private T _value;
        
        public T Value {
            get {
                if (_value != null) {
                    return _value;
                }
                if (_serializedId < 0) {
                    return null;
                }
                TryRestore();
                return _value;
            }
        }

        private void TryRestore() {
            var prefab = Serializer.GetPrefabEntity(_serializedId);
            if (prefab == null) {
                return;
            }
            var targetTr = prefab.transform;
            if (!string.IsNullOrEmpty(_transformChild)) {
                targetTr = prefab.transform.Find(_transformChild);
            }
            if (targetTr != null) {
                _value = targetTr.GetComponent<T>();
                if (_value == null) {
                    _value = prefab.GetComponentInChildren<T>();
                }
            }
        }

        public void Dispose() {
            _value = null;
            _serializedId = -1;
        }

        public void Set(PrefabEntity prefab, T component) {
            SetPrefabEntity(prefab);
            Set(component);
        }

        public void Set(T component) {
            _value = component;
            if (_value == null) {
                _serializedId = -1;
                _transformChild = null;
                return;
            }
            if (_serializedId >= 0) {
                return;
            }
            SetPrefabEntity(PrefabEntity.FindPrefabRoot(_value.transform));
        }

        public void SetPrefabEntity(PrefabEntity prefab) {
            if (prefab == null) {
                return;
            }
            _serializedId = prefab.Metadata.SerializationId;
            if (prefab.transform == _value.transform) {
                _transformChild = null;
                return;
            }
            _transformChild = _value.transform.GetPath();
        }
        
        public CachedUnityComponent(){}

        public CachedUnityComponent(T component) {
            Set(component);
        }
        public CachedUnityComponent(PrefabEntity prefab, T component) {
            Set(prefab, component);
        }
        
        public CachedUnityComponent(SerializationInfo info, StreamingContext context) {
            _serializedId = info.GetValue(nameof(_serializedId), _serializedId);
            _transformChild = info.GetValue(nameof(_transformChild), _transformChild);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_serializedId), _serializedId);
            info.AddValue(nameof(_transformChild), _transformChild);
        }

        public static implicit operator T(CachedUnityComponent<T> reference) {
            return reference.Value;
        }
    }
}
