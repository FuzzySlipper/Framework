using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public struct ActionSpawnComponent : IComponent {
        public int Owner { get; set; }

        public string Prefab { get; }
        public DataEntry Data { get; }

        public ActionSpawnComponent(DataEntry data) : this() {
            Data = data;
            Prefab = data.GetValue<string>(DatabaseFields.Model);
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
