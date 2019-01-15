using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FloatingTextStatusComponent : IComponent, IReceive<StatusUpdate> {
        public int Owner { get; set; }
        public Transform Tr { get; }
        public Vector3 Offset { get; }

        public FloatingTextStatusComponent(Transform tr, Vector3 offset) {
            Tr = tr;
            Offset = offset;
        }

        public void Handle(StatusUpdate arg) {
            FloatingText.Message(arg.Update, Tr.position + Offset, arg.Color);
        }
    }
}
