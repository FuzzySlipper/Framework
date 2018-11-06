using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityContainer : IComponent {

        public int Owner { get; set; }
        public int Limit = -1;
        public event System.Action OnRefreshItemList;

        private List<Entity> _list = new List<Entity>();
        
        public bool IsFull { get { return Limit >= 0 && _list.Count >= Limit; } }
        public int Count { get { return _list.Count; } }
        public virtual bool RequiresInventoryComponent { get { return true; } }
        public Entity this[int index] {
            get {
                if (!_list.HasIndex(index)) {
                    return null;
                }
                return EntityController.GetEntity(_list[index]);
            }
        }

        public bool Contains(Entity item) {
            return _list.Contains(item);
        }

        public virtual bool Add(Entity entity) {
            if (entity == null) {
                return false;
            }
            if (!CanAdd(entity)) {
                return false;
            }
            InventoryItem containerItem = entity.Get<InventoryItem>();
            if (RequiresInventoryComponent) {
                if (containerItem == null) {
                    return false;
                }
                if (containerItem.Inventory != null) {
                    containerItem.Inventory.Remove(entity);
                }
            }
            if (containerItem != null) {
                containerItem.SetContainer(this);
            }
            _list.Add(entity);
            entity.ParentId = Owner;
            var msg = new ContainerStatusChanged(this, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        public bool TryAdd(Entity item) {
            return Add(item);
        }

        public bool Remove(Entity entity) {
            if (!_list.Contains(entity)) {
                return false;
            }
            _list.Remove(entity);
            entity.Get<InventoryItem>(e => e.SetContainer(null));
            entity.ParentId = -1;
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        public virtual bool CanAdd(Entity entity) {
            if (Contains(entity) || IsFull) {
                return false;
            }
            return true;
        }

        public virtual void Clear() {
            _list.Clear();
            OnRefreshItemList.SafeInvoke();
        }

        public virtual void Destroy() {
            for (int i = 0; i < Count; i++) {
                this[i].Destroy();
            }
            _list.Clear();
        }

        public bool ContainsType(string type) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Get<TypeId>().Id == type) {
                    return true;
                }
            }
            return false;
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
