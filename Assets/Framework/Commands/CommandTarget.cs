using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    
    public class CommandTarget : IComponent {

        private Entity _target;
        public Float3? ExplicitPosition;
        public VisibleNode TargetTr;
        public int Owner { get; set; }
        public Entity Target {
            get {
                return _target;
            }
            set {
                _target = value;
                TargetTr = value.GetNode<VisibleNode>();
            }
        }
        public Vector3 GetPosition {
            get {
                if (ExplicitPosition != null) {
                    return ExplicitPosition.Value;
                }
                if (TargetTr != null) {
                    return TargetTr.position;
                }
                return Target?.GetPosition() ?? this.GetEntity().GetPosition();
            }
        }
        public Quaternion GetRotation {
            get {
                if (TargetTr != null) {
                    return TargetTr.rotation;
                }
                return Target?.GetRotation() ?? this.GetEntity().GetRotation();
            }
        }

        public CommandTarget(Entity target, Vector3? explicitPosition, VisibleNode targetTr) {
            _target = target;
            ExplicitPosition = explicitPosition;
            TargetTr = targetTr;
        }

        public CommandTarget(Entity target) {
            _target = target;
            TargetTr = target.GetNode<VisibleNode>();
            ExplicitPosition = null;
        }
    }
}
