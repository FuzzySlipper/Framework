using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ItemInteraction : IComponent {
        public int Owner { get; set; }

        public Func<Entity, bool> OnInteraction;

        public bool Interaction(Entity other) {
            if (OnInteraction != null) {
                return OnInteraction(other);
            }
            return false;
        }
    }
}
