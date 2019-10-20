using System.Collections.Generic;
using System;

namespace PixelComrades {
    public interface IReceive {}
    
    public interface IReceive<in T> : IReceive where T : IEntityMessage {
        void Handle(T arg);
    }

    public interface IReceiveGlobalArray<T> : IReceive where T : IEntityMessage {
        void HandleGlobal(BufferedList<T> arg);
    }

    public interface IReceiveGlobal<in T> : IReceive where T : IEntityMessage {
        void HandleGlobal(T arg);
    }

    //public interface IReceiveEvents<in T> : IReceiveEvents where T : IEntityMessage {
    //    void ReceivedEvent(T msg);
    //}

    //public interface IEntityMessage {}

    public class EventReceiverFilter {
        public IReceive Receiver { get; }
        public System.Type[] Types { get; }
        
        private HashSet<int> _entities;

        public EventReceiverFilter(IReceive receiver, Type[] types) {
            Receiver = receiver;
            Types = types;
            _entities = new HashSet<int>();
        }

        public void CheckAdd(Entity entity) {
            if (_entities.Contains(entity.Id)) {
                return;
            }
            _entities.Add(entity.Id);
            entity.AddObserver(Receiver);
        }

        public void CheckRemove(Entity entity) {
            if (!_entities.Contains(entity.Id)) {
                return;
            }
            for (int i = 0; i < Types.Length; i++) {
                if (entity.HasReference(Types[i])) {
                    return;
                }
            }
            entity.RemoveObserver(Receiver);
        }
    }
}