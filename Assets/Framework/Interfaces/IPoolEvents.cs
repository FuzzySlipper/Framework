using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IPoolEvents {
        void OnPoolSpawned();
        void OnPoolDespawned();
    }

    public interface IOnCreate {
        void OnCreate(PrefabEntity entity);
    }
}