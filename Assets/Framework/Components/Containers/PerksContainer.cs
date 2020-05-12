using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class PerksContainer : IComponent, IEntityContainer {

        public event System.Action OnRefreshItemList;

        private EntityContainer _container;

        public PerksContainer(int limit = -1) {
            _container = new EntityContainer(limit);
        }

        public PerksContainer(SerializationInfo info, StreamingContext context) {
            _container = info.GetValue(nameof(_container), _container);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_container), _container);
        }

        public Entity this[int index] { get { return _container[index]; } }
        public int Count { get { return _container.Count; } }
        public Entity Owner { get { return this.GetEntity(); } }
        public bool IsFull { get { return false; } }

        public bool Contains(Entity item) {
            for (int i = 0; i < Count; i++) {
                if (this[i] == item) {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string id) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Get<PerkConfig>().Id == id) {
                    return true;
                }
            }
            return false;
        }

        public void ContainerSystemSet(Entity item, int index) {
            _container.Add(item);
        }
        
        public int ContainerSystemAdd(Entity item) {
            //var component = item.Get<PerkConfig>();
            return _container.Add(item);
        }

        public bool Remove(Entity entity) {
            if (!_container.Contains(entity)) {
                return false;
            }
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        public void Clear() {
            _container.Clear();
            var msg = new ContainerStatusChanged(this, null);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
        }
    }
}