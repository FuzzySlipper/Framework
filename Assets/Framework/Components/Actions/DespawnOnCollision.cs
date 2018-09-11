using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DespawnOnCollision : IComponent, IReceive<CollisionEvent> {

        public int Owner { get; set; }
        
        public void Handle(CollisionEvent arg) {
            this.GetEntity().Destroy();
        }
    }
}
