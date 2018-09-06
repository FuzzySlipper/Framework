using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class ContainerSystem : SystemBase {

        public static bool TryAddToContainer(EntityContainer container, Entity entity) {
            if (!container.CanAdd(entity)) {
                return false;
            }
            InventoryItem containerItem = entity.Get<InventoryItem>();
            if (container.RequiresInventoryComponent) {
                if (containerItem == null) {
                    return false;
                }
                if (containerItem.Inventory != null) {
                    TryRemoveFromContainer(containerItem.Inventory, entity);
                }
            }
            container.Add(entity, true);
            if (containerItem != null) {
                containerItem.SetContainer(container);
            }
            entity.ParentId = container.Owner;
            var msg = new ContainerStatusChanged(container, entity);
            entity.Post(msg);
            container.GetEntity().Post(msg);
            return true;
        }

        public static bool TryRemoveFromContainer(Entity entity) {
            var containerItem = entity.Get<InventoryItem>();
            if (containerItem == null) {
                return false;
            }
            return containerItem.Inventory != null && TryRemoveFromContainer(containerItem.Inventory, entity);
        }

        public static bool TryRemoveFromContainer(EntityContainer container, Entity entity) {
            if (!container.Contains(entity)) {
                return false;
            }
            container.Remove(entity, true);
            entity.Get<InventoryItem>(e => e.SetContainer(null));
            entity.ParentId = -1;
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            container.GetEntity().Post(msg);
            return true;
        }
    }


    public struct ContainerStatusChanged : IEntityMessage {
        public EntityContainer EntityContainer;
        public Entity Entity;

        public ContainerStatusChanged(EntityContainer entityContainer, Entity entity) {
            EntityContainer = entityContainer;
            Entity = entity;
        }
    }
}
