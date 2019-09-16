using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EntityEventSystem : SystemBase, IReceiveGlobal<EntityDestroyed> {
        
        private GenericPool<EntityEventHub> _eventPool = new GenericPool<EntityEventHub>(25, e => e.Clear());
        private Dictionary<int, EntityEventHub> _entityEvents = new Dictionary<int, EntityEventHub>();
        
        public void AddObserver<T>(int entity, IReceive<T> handler) {
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

        public void RemoveObserver<T>(int entity, IReceive<T> handler) {
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
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.PostSignal(messageType);
        }

        public void Post<T>(int entity, T msg) where T : IEntityMessage {
            if (!_entityEvents.TryGetValue(entity, out var hub)) {
                return;
            }
            hub.Post(msg);
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
#if DEBUGMSGS
            if (msg > 0) {
                DebugLog.Add(DebugId + " posted " + msg + " " + EntitySignals.GetNameAt(msg));
            }
#endif
            World.Get<EntityEventSystem>().PostSignal(entity, msg);
        }

        public static void Post<T>(this Entity entity, T msg) where T : struct, IEntityMessage {
#if DEBUGMSGS
            DebugLog.Add(DebugId + " posted " + msg.GetType().Name);
#endif
            World.Get<EntityEventSystem>().Post<T>(entity, msg);
            World.Enqueue(msg);
        }

        public static void PostAll<T>(this Entity entity, T msg) where T : struct, IEntityMessage {
#if DEBUGMSGS
            DebugLog.Add(DebugId + " posted " + msg.GetType().Name);
#endif
            var evs = World.Get<EntityEventSystem>(); 
            evs.Post<T>(entity, msg);
            World.Enqueue(msg);
            var parent = entity.GetParent();
            while (parent != null) {
                evs.Post(parent, msg);
                parent = parent.GetParent();
            }
        }

        public static void Post(this BaseNode node, int msg) {
            node.Entity.Post(msg);
        }
        
        public static void Post<T>(this BaseNode node, T msg) where T : struct, IEntityMessage {
            node.Entity.Post(msg);
        }

        public static void PostAll<T>(this BaseNode node, T msg) where T : struct, IEntityMessage {
            node.Entity.Post<T>(msg);
        }

        public static void AddObserver<T>(this Entity entity, IReceive<T> handler) {
            World.Get<EntityEventSystem>().AddObserver(entity, handler);
        }

        public static void AddObserver(this Entity entity, IReceive handler) {
            World.Get<EntityEventSystem>().AddObserver(entity, handler);
        }

        public static void AddObserver(this Entity entity, int message, System.Action handler) {
            World.Get<EntityEventSystem>().AddObserver(entity, message, handler);
        }

        public static void RemoveObserver<T>(this Entity entity, IReceive<T> handler) {
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
