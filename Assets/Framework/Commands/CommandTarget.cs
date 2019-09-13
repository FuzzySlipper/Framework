using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class CommandTarget : IComponent {

        private CachedEntity _target = new CachedEntity(-1);
        private Vector3? _explicitPosition;
        private Quaternion? _explicitRotation;
        
        public VisibleNode TargetTr { get; private set; }
        public Entity Target {
            get {
                return _target;
            }
            set {
                _target.Set(value);
                TargetTr = value != null ? value.GetNode<VisibleNode>() : null;
                if (value != null) {
                    _explicitRotation = null;
                    _explicitPosition = null;
                }
            }
        }
        public Vector3 GetPosition {
            get {
                if (TargetTr != null) {
                    return TargetTr.position;
                }
                if (_explicitPosition != null) {
                    return _explicitPosition.Value;
                }
                return Target?.GetPosition() ?? this.GetEntity().GetPosition();
            }
        }
        public Quaternion GetRotation {
            get {
                if (_explicitRotation != null) {
                    return _explicitRotation.Value;
                }
                if (TargetTr != null) {
                    return TargetTr.rotation;
                }
                return Target?.GetRotation() ?? this.GetEntity().GetRotation();
            }
        }
        public bool Valid { get { return _explicitPosition != null || _target != null; } }

        public CommandTarget(Entity target, Vector3? explicitPosition, VisibleNode targetTr) {
            _target.Set(target);
            _explicitPosition = explicitPosition;
            TargetTr = targetTr;
        }

        public CommandTarget(Entity target) {
            _target.Set(target);
            TargetTr = target.GetNode<VisibleNode>();
            _explicitPosition = null;
        }

        public CommandTarget(){}

        public CommandTarget(SerializationInfo info, StreamingContext context) {
            _target = info.GetValue(nameof(_target), _target);
            _explicitPosition = ((SerializedV3) info.GetValue(nameof(_explicitPosition), typeof(SerializedV3))).Value;
            _explicitRotation = ((SerializedQuaternion) info.GetValue(nameof(_explicitRotation), typeof(SerializedQuaternion))).Value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_target), _target);
            info.AddValue(nameof(_explicitPosition), new SerializedV3(_explicitPosition));
            info.AddValue(nameof(_explicitRotation), new SerializedQuaternion(_explicitRotation));
        }
    }
}
