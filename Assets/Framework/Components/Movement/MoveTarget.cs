using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class MoveTarget : IComponent {
        private enum State : byte {
            None,
            ForceLook,
            LookOnly
        }

        private CachedComponent<TransformComponent> _targetTr = new CachedComponent<TransformComponent>();
        private CachedComponent<TransformComponent> _lookTr = new CachedComponent<TransformComponent>();
        private Vector3? _targetV3;
        private State _state;

        public MoveTarget(SerializationInfo info, StreamingContext context) {
            _targetV3 = ((SerializedV3) info.GetValue(nameof(_targetV3), typeof(SerializedV3))).Value;
            _state = info.GetValue(nameof(_state), _state);
            _targetTr = info.GetValue(nameof(_targetTr), _targetTr);
            _lookTr = info.GetValue(nameof(_lookTr), _lookTr);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_targetV3), new SerializedV3(_targetV3));
            info.AddValue(nameof(_targetV3), _targetV3);
            info.AddValue(nameof(_targetTr), _targetTr);
            info.AddValue(nameof(_lookTr), _lookTr);
        }

        public Vector3 GetTargetPosition {
            get {
                if (_targetV3 != null) {
                    return _targetV3.Value;
                }
                if (_targetTr.Value != null && _targetTr.Value.IsValid) {
                    return _targetTr.Value.position;
                }
                return Vector3.zero;
            }
        }
        public Vector3 GetLookTarget {
            get {
                if (_state == State.None) {
                    return GetTargetPosition;
                }
                if (_lookTr.Value != null && _lookTr.Value.IsValid) {
                    return _lookTr.Value.position;
                }
                return GetTargetPosition;
            }
        }

        public bool IsValidMove {
            get { return _state != State.LookOnly && (_targetV3 != null || (_targetTr.Value != null && _targetTr.Value.IsValid)); }
        }

        public bool HasValidLook {
            get { return _state != State.None && (_lookTr.Value != null && _lookTr.Value.IsValid); }
        }

        public void ClearMove() {
            _targetTr.Clear();
            _targetV3 = null;
        }

        public void ClearLook() {
            _lookTr.Clear();
            _state = State.None;
        }

        public void Complete() {
            ClearMove();
        }

        public MoveTarget() {
            _targetTr.Clear();
            _targetV3 = null;
        }

        private bool ExtractLook(Entity target) {
            var tr = target.Get<TransformComponent>();
            _lookTr.Set(tr);
            return tr != null;
        }

        public void SetLookOnlyTarget(Entity target) {
            if (target == null) {
                ClearLook();
                return;
            }
            if (ExtractLook(target)) {
                _state = State.LookOnly;
            }
        }

        public void SetLookTarget(Entity target) {
            if (target == null) {
                ClearLook();
                return;
            }
            if (ExtractLook(target)) {
                _state = State.ForceLook;
            }
        }

        public void SetMoveTarget(SetMoveTarget arg) {
            _targetV3 = arg.V3;
            _targetTr.Set(arg.Tr);
        }

        public void SetMoveTarget(Vector3 target) {
            ClearMove();
            _targetV3 = target;
        }
    }

    public struct SetMoveTarget : IEntityMessage {
        public TransformComponent Tr;
        public Vector3? V3;
        public Entity Owner { get; }
        
        public SetMoveTarget(Entity owner, TransformComponent tr, Vector3? v3) {
            Owner = owner;
            Tr = tr;
            V3 = v3;
        }

        public Vector3 Target { get { return Tr != null ? Tr.position : V3 ?? Vector3.zero; } }
    }

    public struct SetLookTarget : IEntityMessage {
        public Entity Target { get; }
        public bool LookOnly { get; }
        public Entity Owner { get; }

        public SetLookTarget(Entity owner, Entity target, bool lookOnly) {
            Owner = owner;
            Target = target;
            LookOnly = lookOnly;
        }
    }
}
