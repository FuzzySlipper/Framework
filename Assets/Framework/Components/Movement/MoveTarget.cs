using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class MoveTarget : IComponent, IReceive<SetMoveTarget>, IReceive<SetLookTarget> {
        private enum State : byte {
            None,
            ForceLook,
            LookOnly
        }

        private Transform _targetTr;
        private Transform _lookTr;

        private Vector3? _targetV3;
        private PositionComponent _targetPosition;
        private PositionComponent _lookPosition;
        private State _state;

        public MoveTarget(SerializationInfo info, StreamingContext context) {
            _targetV3 = info.GetValue(nameof(_targetV3), _targetV3);
            _targetPosition = info.GetValue(nameof(_targetPosition), _targetPosition);
            _lookPosition = info.GetValue(nameof(_lookPosition), _lookPosition);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_targetV3), _targetV3);
            info.AddValue(nameof(_targetPosition), _targetPosition);
            info.AddValue(nameof(_lookPosition), _lookPosition);
        }

        public Vector3 GetTargetPosition {
            get {
                if (_targetV3 != null) {
                    return _targetV3.Value;
                }
                if (_targetTr != null) {
                    return _targetTr.position;
                }
                return Vector3.zero;
            }
        }
        public Vector3 GetLookTarget {
            get {
                if (_state == State.None) {
                    return GetTargetPosition;
                }
                if (_lookTr != null) {
                    return _lookTr.position;
                }
                return _lookPosition?.Position ?? GetTargetPosition;
            }
        }

        public bool IsValidMove {
            get { return _state != State.LookOnly && (_targetV3 != null || _targetTr != null || _targetPosition != null); }
        }

        public bool HasValidLook {
            get { return _state != State.None && (_lookPosition != null || _lookTr != null); }
        }

        

        public void ClearMove() {
            _targetTr = null;
            _targetV3 = null;
            _targetPosition = null;
        }

        public void ClearLook() {
            _lookPosition = null;
            _lookTr = null;
            _state = State.None;
        }

        public void Complete() {
            ClearMove();
        }

        public MoveTarget() {
            _targetTr = null;
            _targetV3 = null;
        }

        private void ExtractMove(Entity target) {
            _targetPosition = target.Find<PositionComponent>();
            if (_targetPosition != null) {
                return;
            }
            var tr = target.Tr;
            if (tr != null) {
                _targetTr = tr;
            }
            else {
                _targetV3 = target.GetPosition();
            }
        }

        private bool ExtractLook(Entity target) {
            _lookPosition = target.Find<PositionComponent>();
            if (_lookPosition != null) {
                return true;
            }
            _lookTr = target.Tr;
            return _lookTr != null;
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

        public void SetMoveTarget(Entity target) {
            ClearMove();
            ExtractMove(target);
        }

        public void SetMoveTarget(Vector3 target) {
            ClearMove();
            _targetV3 = target;
        }

        public void SetMoveTarget(Transform tr) {
            ClearMove();
            _targetTr = tr;
        }

        public void Handle(SetMoveTarget arg) {
            _targetV3 = arg.V3;
            _targetTr = arg.Tr;
        }

        public void Handle(SetLookTarget arg) {
            if (arg.LookOnly) {
                SetLookOnlyTarget(arg.Target);
            }
            else {
                SetLookTarget(arg.Target);
            }
        }
    }

    public struct SetMoveTarget : IEntityMessage {
        public Transform Tr;
        public Vector3? V3;

        public SetMoveTarget(Transform tr, Vector3? v3) {
            Tr = tr;
            V3 = v3;
        }

        public Vector3 Target { get { return Tr != null ? Tr.position : V3 ?? Vector3.zero; } }
    }

    public struct SetLookTarget : IEntityMessage {
        public Entity Target;
        public bool LookOnly;

        public SetLookTarget(Entity target, bool lookOnly) {
            Target = target;
            LookOnly = lookOnly;
        }
    }
}
