using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class KeyHole : IComponent, IWorldItemInteraction {

        public int PickDifficulty;
        public string KeyString;

        public KeyHole(){}

        public KeyHole(string key, int pickDifficulty = -1) {
            PickDifficulty = pickDifficulty;
            KeyString = key;
        }

        public bool TryLockpick(float lockpick) {
            if (PickDifficulty < 0) {
                return false;
            }
            return lockpick >= PickDifficulty;
        }

        public bool UnlockKey(string key) {
            if (KeyString.CompareCaseInsensitive(key)) {
                return true;
            }
            return false;
        }

        public bool TryInteract(Entity item) {
            return UnlockKey(item.Get<KeyComponent>().KeyId);
        }

        public KeyHole(SerializationInfo info, StreamingContext context) {
            KeyString = info.GetValue(nameof(KeyString), "");
            PickDifficulty = info.GetValue(nameof(PickDifficulty), PickDifficulty);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(KeyString), KeyString);
            info.AddValue(nameof(PickDifficulty), PickDifficulty);
        }
    }
}
