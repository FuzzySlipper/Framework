using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace PixelComrades {

    public class Entity : IEquatable<Entity> {

        private static EntityPool _pool = new EntityPool(100);
        private GenericPool<EntityTagsComponent> _tagPool = new GenericPool<EntityTagsComponent>(50, t => t.ClearEntity());
        private static List<Entity> _toDeleteList = new List<Entity>(25);
        private static TypeComparer _typeComparer = new TypeComparer();

        public static Entity New(string name) {
            var entity = InternalNew(name);
            entity.Id = EntityController.AddEntityToMainList(entity);
            return entity;
        }

        private static Entity InternalNew(string name) {
            var entity = _pool.New();
            entity.Name = name;
            return entity;
        }

        public static Entity Restore(string name, int index) {
            var entity = InternalNew(name);
            entity.Id = EntityController.AddEntityToMainList(entity, index);
            return entity;
        }

        public static void ProcessPendingDeletes() {
            for (int i = 0; i < _toDeleteList.Count; i++) {
                if (_toDeleteList[i].Factory != null && _toDeleteList[i].Factory.TryStore(_toDeleteList[i])) {
#if DEBUG
                    DebugLog.Add("Pooled " + _toDeleteList[i].Name + " to " + _toDeleteList[i].Factory.GetType().Name);
#endif
                    continue;
                }
                _toDeleteList[i].Post(new EntityDestroyed(_toDeleteList[i]));
#if DEBUG
                DebugLog.Add("Deleted " + _toDeleteList[i].Name);
#endif
                _pool.Store(_toDeleteList[i]);
            }
            _toDeleteList.Clear();
        }
        
        private Entity() {}

        public int Id { get; private set;}
        public string Name;
        public int ParentId = -1;
        public bool Pooled = false;
        public IEntityFactory Factory;

        private Dictionary<System.Type, ComponentReference> _components = new Dictionary<Type, ComponentReference>();
        private EntityTagsComponent _tags; 
        
        public string DebugId { get { return Id + "_" + Name; } }
        public EntityTagsComponent Tags {
            get {
                if (_tags == null) {
                    _tags = _tagPool.New();
                    _tags.SetEntity(this);
                }
                return _tags;
            }
        }
        
        public int ComponentCount { get { return _components.Count; } }

        public bool IsDestroyed() {
            return Id < 0 || Pooled;
        }
       
        public void Destroy() {
            if (IsDestroyed() || _toDeleteList.Contains(this)) {
                return;
            }
#if DEBUG
            DebugLog.Add("Added " + DebugId + " to delete list");
#endif
            _toDeleteList.Add(this);
        }

        private void Clear() {
            this.Post(new EntityDisposed(this));
            UnityToEntityBridge.Unregister(this);
            EntityController.FinishDeleteEntity(this);
            _components.Clear();
            Id = -1;
            Name = "Destroyed";
            ClearParent();
            if (_tags != null) {
                _tagPool.Store(_tags);
                _tags = null;
            }
        }

        public void ClearParent() {
            ParentId = -1;
        }

        public bool TryGetReference<T>(out ComponentReference val) {
            if (_components.TryGetValue(typeof(T), out val)) {
                return true;
            }
            val = default(ComponentReference);
            return false;
        }

        public bool TryGetReference(System.Type type, out ComponentReference val) {
            if (_components.TryGetValue(type, out val)) {
                return true;
            }
            val = default(ComponentReference);
            return false;
        }
        
        public bool HasReference<T>() {
            return _components.ContainsKey(typeof(T));
        }

        public bool HasReference(System.Type type) {
            return _components.ContainsKey(type);
        }

        public void CopyKeys(ref System.Type[] array) {
            _components.Keys.CopyTo(array, 0);
        }

        public IEnumerable<ComponentReference> GetAllComponents() {
//            var list = new ComponentReference[_components.Count];
//            _components.Values.CopyTo(list, 0);
//            return list;
            return _components.Values;
        }
        
        public void AddReference(ComponentReference reference) {
            var type = reference.Array.ArrayType;
            if (_components.ContainsKey(type)) {
                _components[type] = reference;
            }
            else {
                _components.Add(type, reference);
            }
        }

        public void RemoveReference(ComponentReference reference) {
            _components.Remove(reference.Array.ArrayType);
        }

        public void RemoveReference(System.Type type) {
            _components.Remove(type);
        }

        public void ClearParent(int matchId) {
            if (ParentId == matchId) {
                ParentId = -1;
            }
        }

        public static implicit operator int(Entity reference) {
            if (reference == null) {
                return -1;
            }
            return reference.Id;
        }

        public override bool Equals(object obj) {
            if (obj is Entity entity) {
                return entity.Id == Id;
            }
            return false;
        }

        public bool Equals(Entity other) {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override string ToString() {
            return $"Entity {Id}:{Name}";
        }

        public static bool operator ==(Entity entity, Entity other) {
            if (object.ReferenceEquals(entity, null)) {
                return object.ReferenceEquals(other, null);
            }
            if (object.ReferenceEquals(other, null)) {
                return false;
            }
            return entity.Id == other.Id;
        }

        public static bool operator !=(Entity entity, Entity other) {
            return !(entity == other);
        }

        private class TypeComparer : Comparer<System.Type> {
            public override int Compare(System.Type x, System.Type y) {
                if (ReferenceEquals(x, y) || (x == null || y == null)) {
                    return 0;
                }
                return x.GetHashCode().CompareTo(y.GetHashCode());
            }
        }

        private class EntityPool {
            private Queue<Entity> _queue;

            public EntityPool(int initialSize) {
                _queue = new Queue<Entity>(initialSize);
            }

            public Entity New() {
                return _queue.Count > 0 ? _queue.Dequeue() : new Entity();
            }

            public void Store(Entity entity) {
                entity.Clear();
                _queue.Enqueue(entity);
            }
        }
    }
}