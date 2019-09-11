using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class ItemInteraction : IComponent {

        public Func<Entity, bool> OnInteraction;

        public bool Interaction(Entity other) {
            if (OnInteraction != null) {
                return OnInteraction(other);
            }
            return false;
        }

        public ItemInteraction(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }
    }
}
