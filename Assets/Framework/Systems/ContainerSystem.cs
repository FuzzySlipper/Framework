using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class ContainerSystem : SystemBase {

        public static bool TryAddToContainer(EntityContainer container, Entity entity) {
            if (!container.CanAdd(entity)) {
                return false;
            }
            var containerItem = entity.Get<ContainerItem>();
            if (containerItem == null) {
                return false;
            }
            if (containerItem.Container != null) {
                TryRemoveFromContainer(containerItem.Container, entity);
            }
            container.Add(entity.Id, true);
            containerItem.Container = container;
            entity.ParentId = container.Owner;
            var msg = new ContainerStatusChanged(container, entity);
            entity.Post(msg);
            container.GetEntity().Post(msg);
            return true;
        }

        public static bool TryRemoveFromContainer(Entity entity) {
            var containerItem = entity.Get<ContainerItem>();
            if (containerItem == null) {
                return false;
            }
            return containerItem.Container != null && TryRemoveFromContainer(containerItem.Container, entity);
        }

        public static bool TryRemoveFromContainer(EntityContainer container, Entity entity) {
            if (!container.Contains(entity.Id)) {
                return false;
            }
            container.Remove(entity.Id, true);
            entity.Get<ContainerItem>(e => e.Container = null);
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
