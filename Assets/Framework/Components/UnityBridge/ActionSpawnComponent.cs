using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public struct ActionSpawnComponent : IComponent {

        public GameObject Prefab;
        public int Owner { get; set; }
        public float Speed { get; }
        public float Rotation { get; }

        public ActionSpawnComponent(GameObject prefab, float speed, float rotation) : this() {
            Prefab = prefab;
            Speed = speed;
            Rotation = rotation;
        }
    }

    public struct AnimTr : IComponent {
        public Transform Tr;
        public int Owner { get; set; }

        public AnimTr(Transform tr) : this() {
            Tr = tr;
        }
    }

    public interface IAnimTr {
        Transform AnimTr { get; }
    }
}
