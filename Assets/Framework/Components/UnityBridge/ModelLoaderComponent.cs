using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Highest)]
    public class ModelLoaderComponent : IComponent, IReceive<EquipmentChanged> {
        private int _owner = -1;
        public int Owner {
            get {
                return _owner;
            }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (!_onlyActiveWhileEquipped) {
                    SpawnModel(null);
                }
            }
        }

        private bool _onlyActiveWhileEquipped;
        private IModelComponent _loadedModel;
        private List<IComponent> _loadedComponents = new List<IComponent>();
        private string _modelName;

        public ModelLoaderComponent(bool onlyActiveWhileEquipped, string modelName) {
            _onlyActiveWhileEquipped = onlyActiveWhileEquipped;
            _modelName = modelName;
        }

        public void Handle(EquipmentChanged arg) {
            if (!_onlyActiveWhileEquipped) {
                return;
            }
            if (arg.Owner == null && _loadedComponents.Count > 0) {
                var entity = this.GetEntity();
                MonoBehaviourToEntity.Unregister(entity);
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
            _loadedModel = model.GetComponent<IModelComponent>();
            if (_loadedModel == null) {
                ItemPool.Despawn(model.gameObject);
                return;
            }
            var entity = this.GetEntity();
            MonoBehaviourToEntity.RegisterToEntity(model.gameObject, entity);
            _loadedComponents.Add(entity.Add(new ModelComponent(_loadedModel)));
            _loadedComponents.Add(entity.Add(new TransformComponent(model.Transform)));
            _loadedComponents.Add(entity.Add(new FloatingTextStatusComponent()));
            var animTr = model.GetComponent<IAnimTr>();
            if (animTr != null) {
                _loadedComponents.Add(entity.Add(new AnimTr(animTr.AnimTr)));
            }
            var rb = model.GetComponent<Rigidbody>();
            if (rb != null) {
                _loadedComponents.Add(entity.Add(new RigidbodyComponent(rb)));
            }
        }
    }
}
