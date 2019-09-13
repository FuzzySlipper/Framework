using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class CachedAnimator : ISerializable {

        private int _serializedId = -1;
        private string _transformChild;
        private int _entityId = -1;
        private System.Type _componentType;
        
        private IAnimator _value;
        
        public IAnimator Value {
            get {
                if (_value != null) {
                    return _value;
                }
                TryRestore();
                return _value;
            }
        }

        private void TryRestore() {
            if (_serializedId >= 0) {
                var prefab = Serializer.GetPrefabEntity(_serializedId);
                if (prefab != null) {
                    if (!string.IsNullOrEmpty(_transformChild)) {
                        var targetTr = prefab.transform.Find(_transformChild);
                        if (targetTr != null) {
                            _value = targetTr.GetComponent<IAnimator>();
                        }
                    }
                    if (_value == null) {
                        _value = prefab.GetComponentInChildren<IAnimator>();
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
                _value = cref.Value.Get() as IAnimator;
            }
        }
        
        public CachedAnimator(IAnimator value) {
            _value = value;
            if (value is UnityEngine.Component unityComponent) {
                var prefab = PrefabEntity.FindPrefabRoot(unityComponent.transform);
                if (prefab != null) {
                    _serializedId = prefab.PrefabId;
                    _transformChild = unityComponent.transform.GetPath();
                }
                else {
                    Debug.LogErrorFormat("{0}: unable to find prefab root on {1}", value.GetType(), unityComponent.gameObject.name);
                }
                return;
            }
            if (value is IComponent ecsComponent) {
                _entityId = ecsComponent.GetEntity();
                _componentType = value.GetType();
                return;
            }
            Debug.LogErrorFormat("{0} is not a valid type of animator", value.GetType());
        }
        
        public CachedAnimator(SerializationInfo info, StreamingContext context) {
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
