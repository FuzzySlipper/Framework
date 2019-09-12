using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    public sealed class ModelLoaderComponent : IComponent, IReceive<EquipmentChanged> {
        
        private bool _onlyActiveWhileEquipped;
        private string _modelName;
        private CachedGenericComponent<IModelComponent> _loadedModel;
        private List<SerializableType> _loadedComponents = new List<SerializableType>();

        public ModelLoaderComponent(bool onlyActiveWhileEquipped, string modelName) {
            _onlyActiveWhileEquipped = onlyActiveWhileEquipped;
            _modelName = modelName;
        }

        public ModelLoaderComponent(SerializationInfo info, StreamingContext context) {
            _onlyActiveWhileEquipped = info.GetValue(nameof(_onlyActiveWhileEquipped), _onlyActiveWhileEquipped);
            _modelName = info.GetValue(nameof(_modelName), _modelName);
            _loadedModel = info.GetValue(nameof(_loadedModel), _loadedModel);
            _loadedComponents = info.GetValue(nameof(_loadedComponents), _loadedComponents);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_onlyActiveWhileEquipped), _onlyActiveWhileEquipped);
            info.AddValue(nameof(_modelName), _modelName);
            info.AddValue(nameof(_loadedModel), _loadedModel);
            info.AddValue(nameof(_loadedComponents), _loadedComponents);
        }
        
        public void Handle(EquipmentChanged arg) {
            if (!_onlyActiveWhileEquipped) {
                return;
            }
            if (arg.Owner == null && _loadedComponents.Count > 0) {
                var entity = this.GetEntity();
                UnityToEntityBridge.Unregister(entity);
                for (int i = 0; i < _loadedComponents.Count; i++) {
                    entity.Remove(_loadedComponents[i]);
                }
                _loadedComponents.Clear();
                _loadedModel = null;
            }
            else if (arg.Owner != null && _loadedModel == null) {
                SpawnModel(arg.Slot.EquipTr);
            }
        }

        private void SpawnModel(Transform parent) {
            var model = ItemPool.Spawn(_modelName);
            if (parent != null) {
                model.transform.SetParentResetPos(parent);
            }
            _loadedModel = new CachedGenericComponent<IModelComponent>(model.GetComponent<IModelComponent>());
            if (_loadedModel == null) {
                ItemPool.Despawn(model.gameObject);
                return;
            }
            var entity = this.GetEntity();
            UnityToEntityBridge.RegisterToEntity(model.gameObject, entity);
            _loadedComponents.Add(entity.Add(new ModelComponent(_loadedModel.Component)).GetType());
            entity.Tr = model.Transform;
            _loadedComponents.Add(entity.Add(new FloatingTextStatusComponent(model.Transform, new Vector3(0, 1.5f, 0))).GetType());
            var rb = model.GetComponent<Rigidbody>();
            if (rb != null) {
                _loadedComponents.Add(entity.Add(new RigidbodyComponent(rb)).GetType());
            }
        }
    }
}
