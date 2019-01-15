using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MoveTarget : IComponent, IReceive<SetMoveTarget> {
        public int Owner { get; set; }

        public Vector3? TargetV3;
        public Transform TargetTr;
        public System.Action OnComplete;

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

        public void Complete() {
            OnComplete.SafeInvoke();
            Clear();
        }

        public MoveTarget() {
            TargetTr = null;
            TargetV3 = null;
        }

        public MoveTarget(Entity target) {
            var tr = target.Tr;
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

        public void Handle(SetMoveTarget arg) {
            TargetV3 = arg.V3;
            TargetTr = arg.Tr;
            OnComplete = arg.OnComplete;
        }
    }

    public struct SetMoveTarget : IEntityMessage {
        public Transform Tr;
        public Vector3? V3;
        public System.Action OnComplete;

        public SetMoveTarget(Transform tr, Vector3? v3, System.Action del) {
            Tr = tr;
            V3 = v3;
            OnComplete = del;
        }

        public Vector3 Target { get { return Tr != null ? Tr.position : V3 ?? Vector3.zero; } }
    }
}
