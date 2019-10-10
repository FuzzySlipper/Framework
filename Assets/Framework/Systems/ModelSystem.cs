using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class ModelSystem : SystemBase, IReceive<EquipmentChanged> {

        public ModelSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(ModelLoaderComponent)
            }));
        }

        public void Handle(EquipmentChanged arg) {
            var loader = arg.Item.Get<ModelLoaderComponent>();
            if (loader != null) {
                HandleLoader(arg, loader);
            }
        }

        private void HandleLoader(EquipmentChanged arg, ModelLoaderComponent loader) {
            if (!loader.OnlyActiveWhileEquipped) {
                return;
            }
            var entity = arg.Item;
            if (arg.Slot == null && loader.LoadedComponents.Count > 0) {
                UnityToEntityBridge.Unregister(entity);
                for (int i = 0; i < loader.LoadedComponents.Count; i++) {
                    entity.Remove(loader.LoadedComponents[i]);
                }
                loader.LoadedComponents.Clear();
                loader.LoadedModel = null;
            }
            else if (arg.Slot != null && loader.LoadedModel == null) {
                var parent = arg.Slot.EquipTr;
                var model = ItemPool.Spawn(loader.ModelName);
                if (parent != null) {
                    model.transform.SetParentResetPos(parent);
                }
                loader.LoadedModel = new CachedGenericComponent<IRenderingComponent>(model.GetComponent<IRenderingComponent>());
                if (loader.LoadedModel == null) {
                    ItemPool.Despawn(model.gameObject);
                    return;
                }

                UnityToEntityBridge.RegisterToEntity(model.gameObject, entity);
                loader.LoadedComponents.Add(entity.Add(new RenderingComponent(loader.LoadedModel.Value)).GetType());
                loader.LoadedComponents.Add(entity.Add(new TransformComponent(model.Transform)).GetType());
                loader.LoadedComponents.Add(
                    entity.Add(new FloatingTextStatusComponent(model.Transform, new Vector3(0, 1.5f, 0))).GetType());
                var rb = model.GetComponent<Rigidbody>();
                if (rb != null) {
                    loader.LoadedComponents.Add(entity.Add(new RigidbodyComponent(rb)).GetType());
                }
            }
        }

    }
}
