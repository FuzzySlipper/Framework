using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class KeyHole : IComponent, IWorldItemInteraction {
        public int Owner { get; set; }

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
            return UnlockKey(item.Get<Key>().KeyId);
        }
    }
}
