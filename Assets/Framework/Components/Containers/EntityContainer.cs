using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityContainer : IComponent {

        public int Owner { get; set; }
        public int Limit = -1;
        public event System.Action OnRefreshItemList;

        private List<Entity> _list = new List<Entity>();
        
        public Entity this[int index] { get { return EntityController.GetEntity(_list[index]); } }
        public bool IsFull { get { return Limit >= 0 && _list.Count >= Limit; } }
        public int Count { get { return _list.Count; } }

        public bool Contains(Entity item) {
            return _list.Contains(item);
        }

        public virtual void Add(Entity item, bool isContainerSystem = false) {
            if (item < 0) {
                return;
            }
            if (!isContainerSystem) {
                ContainerSystem.TryAddToContainer(this, EntityController.GetEntity(item));
                return;
            }
            _list.Add(item);
            OnRefreshItemList.SafeInvoke();
        }

        public bool TryAdd(Entity item) {
            return ContainerSystem.TryAddToContainer(this, item);
        }

        public void Remove(Entity item, bool isContainerSystem = false) {
            if (!isContainerSystem) {
                ContainerSystem.TryRemoveFromContainer(this, EntityController.GetEntity(item));
                return;
            }
            _list.Remove(item);
            OnRefreshItemList.SafeInvoke();
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

}
