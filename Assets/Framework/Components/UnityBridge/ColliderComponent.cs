using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ColliderComponent : IComponent {
        public int Owner { get; set; }

        public Collider Collider;

        public ColliderComponent(Entity owner, Collider collider) {
            Collider = collider;
            Owner = owner;
            owner.Tags.Add(EntityTags.CanUnityCollide);
            LocalCenter = new Vector3(0, Collider.bounds.size.y * 0.5f, 0);
        }

        public Vector3 LocalCenter { get; }
    }
}
