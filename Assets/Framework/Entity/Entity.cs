using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public class Entity : IEquatable<Entity> {

        private static EntityPool _pool = new EntityPool(50);
        private static List<Entity> _toDeleteList = new List<Entity>(25);

        public int Id { get; private set;}
        public string Name;
        public TagsComponent Tags;
        public StatsContainer Stats;
        public int ParentId = -1;
        public bool Pooled = false;

        private EntityEventHub _eventHub;

        public static void ProcessPendingDeletes() {
            for (int i = 0; i < _toDeleteList.Count; i++) {
                _pool.Store(_toDeleteList[i]);
            }
            _toDeleteList.Clear();
        }

        public static Entity New(string name) {
            var entity = _pool.New();
            entity.Id = EntityController.AddEntityToMainList(entity);
            entity.Name = name;
            return entity;
        }

        private Entity() {
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
            _toDeleteList.Add(this);
        }

        public void Destroy(IEntityPool pool) {
            if (_toDeleteList.Contains(this) || IsDestroyed()) {
                return;
            }
            if (pool != null) {
                pool.Store(this);
            }
            else {
                _toDeleteList.Add(this);
            }
        }

        private void Clear() {
            Post(new EntityDisposed(this));
            MonoBehaviourToEntity.Unregister(this);
            EntityController.FinishDeleteEntity(this);
            Id = -1;
            Name = "Destroyed";
            ClearParent();
            _eventHub.Clear();
            Tags.Clear();
            Stats.Clear();
        }

        public void ClearParent() {
            ParentId = -1;
        }

        public void ClearParent(int matchId) {
            if (ParentId == matchId) {
                ParentId = -1;
            }
        }

        public void Post(int msg) {
            _eventHub.PostSignal(msg);
        }

        public void Post<T>(T msg) where T : struct, IEntityMessage {
            _eventHub.Post<T>(msg);
            World.Enqueue(msg);
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
                entity.Clear();
                _queue.Enqueue(entity);
            }
        }
    }
}