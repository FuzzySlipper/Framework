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

        private CachedTransform _targetTr = new CachedTransform();
        private CachedTransform _lookTr = new CachedTransform();
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
                if (_targetTr.Tr != null) {
                    return _targetTr.Tr.position;
                }
                return Vector3.zero;
            }
        }
        public Vector3 GetLookTarget {
            get {
                if (_state == State.None) {
                    return GetTargetPosition;
                }
                if (_lookTr.Tr != null) {
                    return _lookTr.Tr.position;
                }
                return GetTargetPosition;
            }
        }

        public bool IsValidMove {
            get { return _state != State.LookOnly && (_targetV3 != null || _targetTr.Tr != null); }
        }

        public bool HasValidLook {
            get { return _state != State.None && (_lookTr.Tr != null); }
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

        private void ExtractMove(Entity target) {
            var tr = target.Get<TransformComponent>();
            if (tr != null && tr.Value != null) {
                _targetTr.Set(tr);
            }
            else {
                _targetV3 = target.GetPosition();
            }
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
            _targetTr.Set(tr);
        }

        public void Handle(SetMoveTarget arg) {
            _targetV3 = arg.V3;
            _targetTr.Set(arg.Tr);
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
