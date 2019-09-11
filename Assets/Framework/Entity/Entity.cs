using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {

    public class Entity : IEquatable<Entity> {

        private static EntityPool _pool = new EntityPool(100);
        private static List<Entity> _toDeleteList = new List<Entity>(25);

        public int Id { get; private set;}
        public int ParentId = -1;
        public bool Pooled = false;
        public IEntityPool PoolOwner;
        public string Name;
        public Transform Tr;
        public TagsComponent Tags;
        public StatsContainer Stats;

        //public List<ComponentReference> Components = new List<ComponentReference>();
        private SortedList<System.Type, ComponentReference> _components = new SortedList<Type, ComponentReference>(_typeComparer);
        private EntityEventHub _eventHub;
        private static TypeComparer _typeComparer = new TypeComparer();
        public SortedList<Type, ComponentReference> Components { get => _components; }
        public string DebugId { get { return Id + "_" + Name; } }

        public class TypeComparer : Comparer<System.Type> {
            public override int Compare(System.Type x, System.Type y) {
                if (ReferenceEquals(x, y) || (x == null || y == null)) {
                    return 0;
                }
                return x.GetHashCode().CompareTo(y.GetHashCode());
            }
        }

        public static void ProcessPendingDeletes() {
            for (int i = 0; i < _toDeleteList.Count; i++) {
                if (_toDeleteList[i].PoolOwner != null) {
                    _toDeleteList[i].Post(new EntityDestroyed(_toDeleteList[i]));
                    _toDeleteList[i].PoolOwner.Store(_toDeleteList[i]);
#if DEBUG
                    DebugLog.Add("Pooled " + _toDeleteList[i].Name + " to " + _toDeleteList[i].PoolOwner.GetType().Name);
#endif
                    continue;
                }
#if DEBUG
                DebugLog.Add("Deleted " + _toDeleteList[i].Name);
#endif
                _pool.Store(_toDeleteList[i]);
            }
            _toDeleteList.Clear();
        }

        public static Entity New(string name) {
            var entity = InternalNew(name);
            entity.Id = EntityController.AddEntityToMainList(entity);
            return entity;
        }

        private static Entity InternalNew(string name) {
            var entity = _pool.New();
            entity.Name = name;
            entity._eventHub.AddObserver(entity.Stats);
            return entity;
        }

        public static Entity Restore(string name, int index) {
            var entity = InternalNew(name);
            entity.Id = EntityController.AddEntityToMainList(entity, index);
            return entity;
        }

        protected Entity() {
            Tags = new TagsComponent(this);
            Stats = new StatsContainer(this);
            _eventHub = new EntityEventHub();
        }

        public bool IsDestroyed() {
            return Id < 0 || Pooled;
        }

        public void Destroy() {
            if (_toDeleteList.Contains(this) || IsDestroyed()) {
                return;
            }
#if DEBUG
            DebugLog.Add("Added " + DebugId + " to delete list");
#endif
            _toDeleteList.Add(this);
        }

        private void Clear() {
            Post(new EntityDisposed(this));
            UnityToEntityBridge.Unregister(this);
            EntityController.FinishDeleteEntity(this);
            Id = -1;
            Name = "Destroyed";
            ClearParent();
            Tr = null;
            _eventHub.Clear();
            Tags.Clear();
            Stats.Clear();
        }

        public void ClearParent() {
            ParentId = -1;
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

        public void Post(int msg) {
#if DEBUGMSGS
            if (msg > 0) {
                DebugLog.Add(DebugId + " posted " + msg + " " + EntitySignals.GetNameAt(msg));
            }
#endif
            _eventHub.PostSignal(msg);
        }

        public void Post<T>(T msg) where T : struct, IEntityMessage {
#if DEBUGMSGS
            DebugLog.Add(DebugId + " posted " + msg.GetType().Name);
#endif
            _eventHub.Post<T>(msg);
            World.Enqueue(msg);
        }

        public void PostAll<T>(T msg) where T : struct, IEntityMessage {
#if DEBUGMSGS
            DebugLog.Add(DebugId + " posted " + msg.GetType().Name);
#endif
            _eventHub.Post<T>(msg);
            World.Enqueue(msg);
            var parent = this.GetParent();
            while (parent != null) {
                parent._eventHub.Post(msg);
                parent = parent.GetParent();
            }
        }

        public void AddObserver<T>(IReceive<T> handler) {
            _eventHub.AddObserver(handler);
        }

        public void AddObserver(IReceive handler) {
            _eventHub.AddObserver(handler);
        }

        public void AddObserver(int message,  System.Action handler) {
            _eventHub.AddObserver(message, handler);
        }

        public void AddObserver(ISignalReceiver generic) {
            _eventHub.AddObserver(generic);
        }

        public void RemoveObserver<T>(IReceive<T> handler) {
            _eventHub.MessageReceivers.Remove(handler);
        }

        public void RemoveObserver(IReceive handler) {
            _eventHub.MessageReceivers.Remove(handler);
        }

        public void RemoveObserver(int message, System.Action handler) {
            _eventHub.RemoveObserver(message, handler);
        }

        public void RemoveObserver(ISignalReceiver generic) {
            _eventHub.RemoveObserver(generic);
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

        private class EntityPool {
            private Queue<Entity> _queue;

            public EntityPool(int initialSize) {
                _queue = new Queue<Entity>(initialSize);
            }

            public Entity New() {
                return _queue.Count > 0 ? _queue.Dequeue() : new Entity();
            }

            public void Store(Entity entity) {
                entity.Post(new EntityDestroyed(entity));
                entity.Clear();
                _queue.Enqueue(entity);
            }
        }
    }
}