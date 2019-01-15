using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class VelocityMoverProvider : IDataFactory<VelocityMover> {
        public void AddComponent(Entity entity, DataEntry data) {
            entity.Add(new VelocityMover());
        }
    }

    public class CollisionCheckForwardProvider : IDataFactory<CollisionCheckForward> {
        public void AddComponent(Entity entity, DataEntry data) {
            entity.Add(new CollisionCheckForward(data.TryGetValue<float>(DatabaseFields.CollisionDistance, 10f)));
        }
    }
    
}
