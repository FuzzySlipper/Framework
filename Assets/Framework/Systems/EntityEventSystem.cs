using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EntityEventSystem : SystemBase, IReceiveGlobal<EntityDestroyed> {
        
        private GenericPool<EntityEventHub> _eventPool = new GenericPool<EntityEventHub>(25, e => e.Clear());
        private Dictionary<int, EntityEventHub> _entityEvents = new Dictionary<int, EntityEventHub>();
        
        public void AddObserver<T>(int entity, IReceive<T> handler) where T : IEntityMessage {
            GetHub(entity).AddObserver(handler);
        }

        public void AddObserver(int entity, IReceive handler) {
            GetHub(entity).AddObserver(handler);
        }

        public void AddObserver(int entity, int messageType, System.Action handler) {
            GetHub(entity).AddObserver(messageType, handler);
        }

        public void RemoveObserver(int entity, int messageType, System.Action handler) {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.RemoveObserver(messageType, handler);
            if (hub.Count == 0) {
                _entityEvents.Remove(entity);
                _eventPool.Store(hub);
            }
        }

        public void RemoveObserver<T>(int entity, IReceive<T> handler) where T : IEntityMessage {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.RemoveObserver(handler);
            if (hub.Count == 0) {
                _entityEvents.Remove(entity);
                _eventPool.Store(hub);
            }
        }

        public void RemoveObserver(int entity, IReceive handler) {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.RemoveObserver(handler);
            if (hub.Count == 0) {
                _entityEvents.Remove(entity);
                _eventPool.Store(hub);
            }
        }

        public void PostSignal(int entity, int messageType) {
#if DEBUGMSGS
            if (msg > 0) {
                DebugLog.Add(DebugId + " posted " + msg + " " + EntitySignals.GetNameAt(msg));
            }
#endif
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.PostSignal(messageType);
        }

        public void Post<T>(int entity, T msg) where T : struct, IEntityMessage {
#if DEBUGMSGS
            DebugLog.Add(DebugId + " posted " + msg.GetType().Name);
#endif
            World.Enqueue(msg);
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.Post(msg);
        }

        public void Post<T>(T msg) where T : struct, IEntityMessage {
            World.Enqueue(msg);
        }

        public void PostNoGlobal<T>(int entity, T msg) where T : struct, IEntityMessage {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.Post(msg);
        }

        public void PostAll<T>(Entity entity, T msg) where T : struct, IEntityMessage {
#if DEBUGMSGS
            DebugLog.Add(DebugId + " posted " + msg.GetType().Name);
#endif
            World.Enqueue(msg);
            var targetEntity = entity;
            while (targetEntity != null) {
                if (_entityEvents.TryGetValue(targetEntity, out var hub)) {
                    hub.Post(msg);
                }
                targetEntity = targetEntity.GetParent();
            }
        }
        
        private EntityEventHub GetHub(int entity) {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                hub = _eventPool.New();
                _entityEvents.Add(entity, hub);
            }
            return hub;
        }

        private void RemoveHub(int entity) {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            _entityEvents.Remove(entity);
            _eventPool.Store(hub);
        }

        public void HandleGlobal(EntityDestroyed arg) {
            RemoveHub(arg.Entity);
        }
    }

    public static class EntityEventExtensions {

        public static void Post(this Entity entity, int msg) {
            World.Get<EntityEventSystem>().PostSignal(entity, msg);
        }

        public static void Post<T>(this Entity entity, T msg) where T : struct, IEntityMessage {
            World.Get<EntityEventSystem>().Post<T>(entity, msg);
        }

        public static void PostNoGlobal<T>(this Entity entity, T msg) where T : struct, IEntityMessage {
            World.Get<EntityEventSystem>().PostNoGlobal<T>(entity, msg);
        }
        
        public static void PostAll<T>(this Entity entity, T msg) where T : struct, IEntityMessage {
            World.Get<EntityEventSystem>().PostAll<T>(entity, msg);
        }

        public static void Post<T>(this IEntityTemplate entityTemplate, T msg) where T : struct, IEntityMessage {
            World.Get<EntityEventSystem>().Post<T>(entityTemplate.Entity, msg);
        }
        
        public static void AddObserver<T>(this Entity entity, IReceive<T> handler) where T : IEntityMessage {
            World.Get<EntityEventSystem>().AddObserver(entity, handler);
        }

        public static void AddObserver(this Entity entity, IReceive handler) {
            World.Get<EntityEventSystem>().AddObserver(entity, handler);
        }

        public static void AddObserver(this Entity entity, int message, System.Action handler) {
            World.Get<EntityEventSystem>().AddObserver(entity, message, handler);
        }

        public static void RemoveObserver<T>(this Entity entity, IReceive<T> handler) where T : IEntityMessage {
            World.Get<EntityEventSystem>().RemoveObserver<T>(entity, handler);
        }

        public static void RemoveObserver(this Entity entity, IReceive handler) {
            World.Get<EntityEventSystem>().RemoveObserver(entity, handler);
        }

        public static void RemoveObserver(this Entity entity, int message, System.Action handler) {
            World.Get<EntityEventSystem>().RemoveObserver(entity, message, handler);
        }
    }
}
