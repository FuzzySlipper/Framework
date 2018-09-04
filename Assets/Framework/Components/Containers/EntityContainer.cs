using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ContainerItem : IComponent {
        public int StackAmount;
        public EntityContainer Container;

        public int Owner { get; set; }

        public ContainerItem(int owner, int stackAmount = 1) {
            StackAmount = stackAmount;
            Owner = owner;
        }
    }

    
    public class EntityContainer : IComponent {

        public int Owner { get; set; }
        public int Limit = -1;
        public event System.Action OnRefreshItemList;

        private List<int> _list = new List<int>();
        
        public Entity this[int index] { get { return EntityController.GetEntity(_list[index]); } }
        public bool IsFull { get { return Limit >= 0 && _list.Count >= Limit; } }
        public int Count { get { return _list.Count; } }

        public bool Contains(int item) {
            return _list.Contains(item);
        }

        public virtual void Add(int item, bool isContainerSystem = false) {
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

        public void Remove(int item, bool isContainerSystem = false) {
            if (!isContainerSystem) {
                ContainerSystem.TryRemoveFromContainer(this, EntityController.GetEntity(item));
                return;
            }
            _list.Remove(item);
            OnRefreshItemList.SafeInvoke();
        }

        public virtual bool CanAdd(Entity entity) {
            if (Contains(entity.Id) || IsFull) {
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
