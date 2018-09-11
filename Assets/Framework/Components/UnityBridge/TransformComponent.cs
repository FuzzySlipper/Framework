using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct TransformComponent : IComponent {

        public int Owner { get; set; }
        public Transform Tr;

        public TransformComponent(Transform tr) : this() {
            Tr = tr;
        }

        public static implicit operator Transform(TransformComponent tr) {
            return tr.Tr;
        }

        public Vector3 position {
            get { return Tr.position; }
            set { Tr.position = value; }
        }

        public Quaternion rotation {
            get { return Tr.rotation; }
            set { Tr.rotation = value; }
        }

        public Vector3 forward { get { return Tr.forward; } }
    }
}
