using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class CommandTarget : IComponent {

        private CachedEntity _target = new CachedEntity(-1);
        private Vector3? _explicitPosition;
        
        public CharacterTemplate TargetChar { get; private set; }
        public Entity Target {
            get {
                return _target;
            }
            set {
                _target.Set(value);
                TargetChar = value != null ? value.GetTemplate<CharacterTemplate>() : null;
                if (value != null) {
                    _explicitPosition = null;
                }
            }
        }
        public Vector3 GetPosition {
            get {
                if (TargetChar != null) {
                    return TargetChar.Tr.position;
                }
                if (_explicitPosition != null) {
                    return _explicitPosition.Value;
                }
                return Target?.GetPosition() ?? this.GetEntity().GetPosition();
            }
        }
        public Point3 GetPositionP3 {
            get {
                if (TargetChar != null) {
                    return TargetChar.Position;
                }
                if (_explicitPosition != null) {
                    return _explicitPosition.Value.ToCellGridP3();
                }
                return (Target?.GetPosition() ?? this.GetEntity().GetPosition()).ToCellGridP3();
            }
        }
        public bool Valid { get { return _explicitPosition != null || _target != null; } }

        public Quaternion GetLookAtTarget(Vector3 start) {
            Vector3 relativePos = GetPosition - start;
            return Quaternion.LookRotation(relativePos, Vector3.up);
        }

        public CommandTarget(Entity target, Vector3? explicitPosition, CharacterTemplate targetChar) {
            _target.Set(target);
            _explicitPosition = explicitPosition;
            TargetChar = targetChar;
        }

        public CommandTarget(Entity target) {
            _target.Set(target);
            TargetChar = target.GetTemplate<CharacterTemplate>();
            _explicitPosition = null;
        }

        public void Set(Vector3 explicitPosition) {
            _explicitPosition = explicitPosition;
            _target.Clear();
            TargetChar = null;
        }

        public void Set(CharacterTemplate target) {
            _target.Set(target);
            TargetChar = target;
            _explicitPosition = null;
        }

        public CommandTarget(){}

        public CommandTarget(SerializationInfo info, StreamingContext context) {
            _target = info.GetValue(nameof(_target), _target);
            _explicitPosition = ((SerializedV3) info.GetValue(nameof(_explicitPosition), typeof(SerializedV3))).Value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_target), _target);
            info.AddValue(nameof(_explicitPosition), new SerializedV3(_explicitPosition));
        }
    }
}
