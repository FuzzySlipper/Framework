using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public struct KeyComponent : IComponent {
        public string KeyId { get; }

        public KeyComponent(string keyId) : this() {
            KeyId = keyId;
        }

        public bool TryUse(Entity other) {
            return TryUse(other.Get<KeyHole>());
        }

        public bool TryUse(KeyHole hole) {
            return hole.UnlockKey(KeyId);
        }

        public KeyComponent(SerializationInfo info, StreamingContext context) {
            KeyId = info.GetValue(nameof(KeyId), "");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(KeyId), KeyId);
        }
    }
}
