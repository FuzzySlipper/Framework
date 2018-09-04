using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct DespawnOnCollision : IComponent, IReceive<CollisionEvent> {

        public int Owner { get; set; }

        public DespawnOnCollision(int owner) {
            Owner = owner;
        }

        public void Handle(CollisionEvent arg) {

        }
    }
}
