using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ColliderComponent : IComponent {
        public int Owner { get; set; }
        public Collider Collider;

        public ColliderComponent(Entity owner, Collider collider) {
            Collider = collider;
            Owner = owner;
            owner.Tags.Add(EntityTags.CanUnityCollide);
        }
    }
}
