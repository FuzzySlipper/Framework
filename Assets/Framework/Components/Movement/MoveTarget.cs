using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MoveTarget : IComponent {
        public int Owner { get; set; }

        public Vector3? TargetV3;
        public Transform TargetTr;

        public Vector3 GetTargetPosition {
            get {
                if (TargetV3 != null) {
                    return TargetV3.Value;
                }
                if (TargetTr != null) {
                    return TargetTr.position;
                }
                return Vector3.zero;
            }
        }

        public bool IsValid {
            get { return TargetV3 != null || TargetTr != null; }
        }

        public void Clear() {
            TargetTr = null;
            TargetV3 = null;
        }

        public MoveTarget(int owner) {
            Owner = owner;
        }
    }
}
