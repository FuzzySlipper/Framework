using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class CachedTransformReference : IDisposable  {

        private int _serializedId = -1;
        private string _transformChild;
        private Transform _transform;
        
        public Transform Tr {
            get {
                if (_transform != null) {
                    return _transform;
                }
                if (_serializedId < 0) {
                    return null;
                }
                TryRestore();
                return _transform;
            }
        }

        private void TryRestore() {
            var prefab = Serializer.GetPrefabEntity(_serializedId);
            if (prefab == null) {
                return;
            }
            _transform = prefab.transform;
            if (!string.IsNullOrEmpty(_transformChild)) {
                _transform = prefab.transform.Find(_transformChild);
            }
        }

        public void Dispose() {
            _transform = null;
        }

        public void Set(PrefabEntity prefab, Transform tr) {
            SetPrefabEntity(prefab);
            Set(tr);
        }

        public void Set(Transform tr) {
            _transform = tr;
            if (_transform == null) {
                _serializedId = -1;
                _transformChild = null;
                return;
            }
            if (_serializedId >= 0) {
                return;
            }
            var checkTr = _transform.transform;
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
                _serializedId = -1;
                return;
            }
            _serializedId = prefab.Metadata.SerializationId;
            if (prefab.transform == _transform.transform) {
                _transformChild = null;
                return;
            }
            _transformChild = _transform.transform.GetPath();
        }
        
        public CachedTransformReference(){}

        public CachedTransformReference(PrefabEntity prefab, Transform tr) {
            Set(prefab, tr);
        }

        public CachedTransformReference(Transform tr) {
            Set(tr);
        }

        public CachedTransformReference(PrefabEntity prefab) {
            Set(prefab, prefab.Transform);
        }
        
        public CachedTransformReference(SerializationInfo info, StreamingContext context) {
            _serializedId = info.GetValue(nameof(_serializedId), _serializedId);
            _transformChild = info.GetValue(nameof(_transformChild), _transformChild);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_serializedId), _serializedId);
            info.AddValue(nameof(_transformChild), _transformChild);
        }

        public static implicit operator Transform(CachedTransformReference reference) {
            return reference.Tr;
        }
    }
}
