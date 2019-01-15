using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FloatingTextCombatComponent : IComponent, IReceive<CombatStatusUpdate> {
        public int Owner { get; set; }
        public Transform Tr { get; }
        public Vector3 Offset { get; }

        public FloatingTextCombatComponent(Transform tr, Vector3 offset) {
            Tr = tr;
            Offset = offset;
        }

        public void Handle(CombatStatusUpdate arg) {
            FloatingText.Message(arg.Update, Tr.position + Offset, arg.Color);
        }
    }
}
