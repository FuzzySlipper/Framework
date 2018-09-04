using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class BroadcastSystem : SystemBase {

        public void Post<T>(int owner, T msg) where T : IEntityMessage {

        }

    }

    [Priority(Priority.Lowest)]
    public class EventBroadcaster<T> : IComponent, IReceive<T> where T : IEntityMessage {

        public int Owner { get; set; }

        public void Handle(T arg) {
            World.Get<BroadcastSystem>().Post(Owner, arg);
        }

        public EventBroadcaster(int owner) {
            Owner = owner;
        }
    }
}
