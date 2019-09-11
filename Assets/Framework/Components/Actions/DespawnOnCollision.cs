using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class DespawnOnCollision : IComponent, IReceive<CollisionEvent>, IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent> {

        public void Handle(CollisionEvent arg) {
            this.GetEntity().Destroy();
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            this.GetEntity().Destroy();
        }

        public void Handle(PerformedCollisionEvent arg) {
            this.GetEntity().Destroy();
        }

        public DespawnOnCollision(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
