using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class ItemInventory : IComponent, IEntityContainer {
        
        public event System.Action OnRefreshItemList;

        private ManagedArray<CachedEntity> _array;
        private Dictionary<int, CachedEntity> _entityLookup = new Dictionary<int, CachedEntity>();
        private static GenericPool<CachedEntity> _cachePool = new GenericPool<CachedEntity>(50, c => c.Clear());
        
        public bool IsFull { get { return _array.IsFull; } }
        public int Count { get { return _array.UsedCount; } }
        public int Max { get; private set; }
        public Entity Owner { get { return this.GetEntity(); } }
        public Entity this[int index] {
            get {
                return _array[index];
            }
        }

        public ItemInventory(int size) {
            _array = new ManagedArray<CachedEntity>(size);
            Max = size;
        }

        public ItemInventory(SerializationInfo info, StreamingContext context) {
            _array = info.GetValue(nameof(_array), _array);
            Max = info.GetValue(nameof(Max), Max);
            foreach (CachedEntity cachedEntity in _array) {
                _entityLookup.AddOrUpdate(cachedEntity.EntityId, cachedEntity);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_array), _array);
            info.AddValue(nameof(Max), Max);
        }

        public bool Contains(Entity item) {
            return _entityLookup.ContainsKey(item);
        }

        public bool Add(Entity entity) {
            if (!CanAdd(entity)) {
                return false;
            }
            if (!SetupNewEntity(entity)) {
                return false;
            }
            entity.Get<InventoryItem>().Index = _array.Add(GetCachedEntity(entity));
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        private CachedEntity GetCachedEntity(Entity entity) {
            if (!_entityLookup.TryGetValue(entity, out var cached)) {
                cached = _cachePool.New();
                cached.Set(entity);
                _entityLookup.Add(entity, cached);
            }
            return cached;
        }
        
        public bool Add(Entity entity, int index) {
            if (!CanAdd(entity)) {
                return false;
            }
            if (_array[index] != null) {
                return false;
            }
            if (!SetupNewEntity(entity)) {
                return false;
            }
            _array.Set(index, GetCachedEntity(entity));
            entity.Get<InventoryItem>().Index = index;
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        private bool SetupNewEntity(Entity entity) {
            InventoryItem containerItem = entity.Get<InventoryItem>();
            if (containerItem == null) {
                return false;
            }
            if (containerItem.Inventory != null) {
                containerItem.Inventory.Remove(entity);
            }
            containerItem.SetContainer(this);
            entity.ParentId = this.GetEntity();
            var msg = new ContainerStatusChanged(this, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            return true;
        }

        public bool TryAdd(Entity item) {
            return Add(item);
        }

        public bool Remove(Entity entity) {
            if (!Contains(entity)) {
                return false;
            }
            _array.Remove(entity);
            if (_entityLookup.TryGetValue(entity, out var cached)) {
                _cachePool.Store(cached);
                _entityLookup.Remove(entity);
            }
            ProcessEntityRemoval(entity);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        private void ProcessEntityRemoval(Entity entity) {
            entity.Get<InventoryItem>()?.SetContainer(null);
            entity.ParentId = -1;
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
        }

        public  bool CanAdd(Entity entity) {
            if (entity == null || Contains(entity) || IsFull) {
                return false;
            }
            return entity.HasComponent<InventoryItem>();
        }

        public void Clear() {
            _array.Clear();
            foreach (var entry in _entityLookup) {
                _cachePool.Store(entry.Value);
            }
            _entityLookup.Clear();
            OnRefreshItemList.SafeInvoke();
        }

        public void Destroy() {
            for (int i = 0; i < Count; i++) {
                this[i].Destroy();
            }
            _array = null;
        }

        public void ContainerChanged() {
            OnRefreshItemList.SafeInvoke();
        }

        public bool ContainsType(string type) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Get<TypeId>().Id == type) {
                    return true;
                }
            }
            return false;
        }
       
        public bool TrySwap(int index1, int index2) {
            var item1 = this[index1];
            var item2 = this[index2];
            if (item1 == null || item2 == null) {
                return false;
            }
            item1.Get<InventoryItem>().Index = index2;
            item2.Get<InventoryItem>().Index = index1;
            _array[index1] = GetCachedEntity(item2);
            _array[index2] = GetCachedEntity(item1);
            return true;
        }

        public bool TryChangeIndex(int indexOld, int indexNew, InventoryItem item) {
            var item1 = this[indexOld];
            if (item1 == null) {
                return false;
            }
            _array.Remove(indexOld);
            _array.Set(indexNew, GetCachedEntity(item1));
            item.Index = indexNew;
            return true;
        }

        public bool TryReplace(Entity entity, int index) {
            if (entity == null || !_array.HasIndex(index)) {
                return false;
            }
            if (!CanAdd(entity) || !SetupNewEntity(entity)) {
                return false;
            }
            ProcessEntityRemoval(this[index]);
            _array[index] = GetCachedEntity(entity);
            return true;
        }
    }
}
