using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct Key : IComponent {
        public int Owner { get; set; }
        public string KeyId { get; }

        public Key(string keyId) : this() {
            KeyId = keyId;
        }

        public bool TryUse(Entity other) {
            return TryUse(other.Get<KeyHole>());
        }

        public bool TryUse(KeyHole hole) {
            return hole.UnlockKey(KeyId);
        }
    }
}
