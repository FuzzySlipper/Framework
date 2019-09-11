using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class CachedUnityComponentReference<T> : IDisposable where T : UnityEngine.Component {

        private int _serializedId = -1;
        private string _transformChild;
        private T _component;
        
        public T Component {
            get {
                if (_component != null) {
                    return _component;
                }
                if (_serializedId < 0) {
                    return null;
                }
                TryRestore();
                return _component;
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
                _component = targetTr.GetComponent<T>();
                if (_component == null) {
                    _component = prefab.GetComponentInChildren<T>();
                }
            }
        }

        public void Dispose() {
            _component = null;
            _serializedId = -1;
        }

        public void Set(PrefabEntity prefab, T component) {
            SetPrefabEntity(prefab);
            Set(component);
        }

        public void Set(T component) {
            _component = component;
            if (_component == null) {
                _serializedId = -1;
                _transformChild = null;
                return;
            }
            if (_serializedId >= 0) {
                return;
            }
            var checkTr = _component.transform;
            WhileLoopLimiter.ResetInstance();
            while (WhileLoopLimiter.InstanceAdvance()) {
                if (checkTr == null) {
                    break;
                }
                var prefab = checkTr.GetComponent<PrefabEntity>();
                if (prefab != null) {
                    SetPrefabEntity(prefab);
                    break;
                }
                if (checkTr.parent == null) {
                    SetPrefabEntity(checkTr.GetComponentInChildren<PrefabEntity>());
                    break;
                }
                checkTr = checkTr.parent;
            }
        }

        public void SetPrefabEntity(PrefabEntity prefab) {
            if (prefab == null) {
                return;
            }
            _serializedId = prefab.Metadata.SerializationId;
            if (prefab.transform == _component.transform) {
                _transformChild = null;
                return;
            }
            _transformChild = _component.transform.GetPath();
        }
        
        public CachedUnityComponentReference(){}

        public CachedUnityComponentReference(T component) {
            Set(component);
        }
        public CachedUnityComponentReference(PrefabEntity prefab, T component) {
            Set(prefab, component);
        }
        
        public CachedUnityComponentReference(SerializationInfo info, StreamingContext context) {
            _serializedId = info.GetValue(nameof(_serializedId), _serializedId);
            _transformChild = info.GetValue(nameof(_transformChild), _transformChild);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_serializedId), _serializedId);
            info.AddValue(nameof(_transformChild), _transformChild);
        }

        public static implicit operator T(CachedUnityComponentReference<T> reference) {
            return reference.Component;
        }
    }
}
