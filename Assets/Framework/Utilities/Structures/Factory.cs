using UnityEngine;

namespace PixelComrades {
    public abstract class Factory : ScriptableObject, IMustBeWipedOut {
        public virtual T Spawn<T>(Vector3 pos, Quaternion rot) where T : class {
            return default(T);
        }

        public virtual Transform Spawn(int id) {
            return null;
        }
    }
}