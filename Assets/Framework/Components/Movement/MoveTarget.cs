using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MoveTarget : IComponent, IReceive<SetTarget> {
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

        public MoveTarget() {
            TargetTr = null;
            TargetV3 = null;
        }

        public MoveTarget(Entity target) {
            var tr = target.Get<TransformComponent>().Tr;
            if (tr != null) {
                TargetTr = tr;
            }
            else {
                TargetV3 = target.GetPosition();
            }
        }

        public MoveTarget(Vector3 target) {
            TargetV3 = target;
        }

        public MoveTarget(Transform tr) {
            TargetTr = tr;
        }

        public void Handle(SetTarget arg) {
            TargetV3 = arg.V3;
            TargetTr = arg.Tr;
        }
    }

    public struct SetTarget : IEntityMessage {
        public Transform Tr;
        public Vector3? V3;

        public SetTarget(Transform tr, Vector3? v3) {
            Tr = tr;
            V3 = v3;
        }

        public Vector3 Target { get { return Tr != null ? Tr.position : V3 ?? Vector3.zero; } }
    }
}
