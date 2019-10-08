using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class ItemInventory : IComponent, IEntityContainer {
        
        private ManagedArray<CachedEntity> _array;
        private Dictionary<int, CachedEntity> _entityLookup = new Dictionary<int, CachedEntity>();
        private static GenericPool<CachedEntity> _cachePool = new GenericPool<CachedEntity>(50, c => c.Clear());
        
        public bool IsFull { get { return _array.IsFull; } }
        public int Count { get { return _array.UsedCount; } }
        public int Max { get; private set; }
        public Entity Owner { get { return this.GetEntity(); } }
        public Entity this[int index] {
            get {
                if (index < 0 || index >= _array.Max) {
                    return null;
                }
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

        public int ContainerSystemAdd(Entity entity) {
            return _array.Add(GetCachedEntity(entity));
        }
        
        private CachedEntity GetCachedEntity(Entity entity) {
            if (!_entityLookup.TryGetValue(entity, out var cached)) {
                cached = _cachePool.New();
                cached.Set(entity);
                _entityLookup.Add(entity, cached);
            }
            return cached;
        }
        
        public void ContainerSystemSet(Entity entity, int index) {
            var cached = GetCachedEntity(entity);
            for (int i = 0; i < _array.ArrayCount; i++) {
                if (_array[i] == cached) {
                    _array.Remove(i);
                }
            }
            _array.Set(index, cached);
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
            entity.Get<InventoryItem>()?.SetContainer(null);
            entity.ParentId = -1;
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            return true;
        }

        public void Clear() {
            _array.Clear();
            foreach (var entry in _entityLookup) {
                _cachePool.Store(entry.Value);
            }
            _entityLookup.Clear();
        }

        public void Destroy() {
            for (int i = 0; i < Count; i++) {
                this[i].Destroy();
            }
            _array = null;
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
