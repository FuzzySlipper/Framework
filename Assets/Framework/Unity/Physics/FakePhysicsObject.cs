using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public interface IFakePhysicsCollision {
        void Collision(Entity entity);
    }

    public class FakePhysicsObject : MonoBehaviour, IOnCreate, IPoolEvents, ISystemUpdate {

        [SerializeField] private float _gravitySpeed = 5f;
        [SerializeField] private float _pullSpeed = 1f;
        [SerializeField] private float _floorDistance = 0.2f;
        [SerializeField] private float _collisionRadius = 0.25f;
        [SerializeField] private bool _playerOnly = true;
        [SerializeField] private FloatRange _pullRange = new FloatRange(1, 6);
        [SerializeField] private TweenArc _arcMover = new TweenArc(EasingTypes.Linear, true);
        [SerializeField] private LayerMask _collisionMask = LayerMasks.DefaultCollision;
        [SerializeField] private LayerMask _pullMask = LayerMasks.DefaultCollision;
        [SerializeField] private float _offset = 0.25f;

        private IFakePhysicsCollision[] _collisions;
        private Vector3 _target;
        private Vector3 _lastGood;
        private Transform _pullTarget;
        private TweenV3 _floorLerp;
        private State _state = State.Disabled;
        private Collider[] _tempColliders = new Collider[15];
        
        public bool Unscaled { get { return false; } }

        public void OnCreate(PrefabEntity entity) {
            _collisions = GetComponentsInChildren<IFakePhysicsCollision>(true);
            _floorLerp = new TweenV3(Vector3.zero, Vector3.zero, _floorDistance, EasingTypes.SinusoidalInOut, false);
        }

        public void OnPoolSpawned() {
            _state = State.Falling;
        }

        public void OnPoolDespawned() {
            _state = State.Disabled;
        }

        public void Throw(Vector3 position) {
            _target = position + (Vector3.up * _offset);
            _arcMover.Restart(transform, _target);
            _state = State.Throwing;
        }

        public void OnSystemUpdate(float dt) {
            switch (_state) {
                case State.FinishingFall:
                    if (!FoundFloor()) {
                        _state = State.Falling;
                    }
                    else {
                        transform.position = _floorLerp.Get();
                        if (!_floorLerp.Active) {
                            _state = State.Disabled;
                        }
                    }
                    break;
                case State.Pulling:
                    if (_pullTarget == null) {
                        CheckState();
                        break;
                    }
                    var dir = (_pullTarget.position - transform.position);
                    var distance = dir.magnitude;
                    if (distance > _pullRange.Max * 1.5f) {
                        CheckState();
                    }
                    else {
                        transform.position = Vector3.MoveTowards(transform.position, _pullTarget.position, Mathf.Lerp(_pullSpeed * 0.25f, _pullSpeed, Mathf.InverseLerp(_pullRange.Max, _pullRange.Min, distance)) * dt);
                    }
                    break;
                case State.Falling:
                    transform.Translate(-transform.up * _gravitySpeed * dt);
                    CheckState();
                    break;
                case State.Throwing:
                    if (!_arcMover.Active) {
                        CheckState();
                        break;
                    }
                    _arcMover.Get(transform);
                    break;
            }
            if (_collisionRadius > 0 && _collisions.Length > 0) {
                var cnt = Physics.OverlapSphereNonAlloc(transform.position, _collisionRadius, _tempColliders, _collisionMask);
                for (int c = 0; c < cnt; c++) {
                    var collEntity = UnityToEntityBridge.GetEntity(_tempColliders[c]);
                    if (collEntity == null) {
                        continue;
                    }
                    for (int i = 0; i < _collisions.Length; i++) {
                        _collisions[i].Collision(collEntity);
                    }
                }
            }
            if (_pullRange.Max <= 0) {
                return;
            }
            switch (_state) {
                case State.Pulling:
                case State.Throwing:
                    return;
            }
            var pullCnt = Physics.OverlapSphereNonAlloc(transform.position, _pullRange.Max, _tempColliders, _pullMask);
            for (int c = 0; c < pullCnt; c++) {
                var coll = _tempColliders[c];
                if (_playerOnly && !coll.transform.CompareTag(StringConst.TagPlayer)) {
                    continue;
                }
                _pullTarget = _tempColliders[0].transform;
                _state = State.Pulling;
                break;
            }
        }

        private void CheckState() {
            if (!FoundFloor()) {
                _state = State.Falling;
                if (!Physics.Raycast(transform.position, Vector3.down, 1000f, LayerMasks.Floor)) {
                    transform.position = _lastGood;
                }
                else {
                    _lastGood = transform.position;
                }
            }
            else {
                if (Vector3.Distance(_target, transform.position) > _offset + (_floorDistance * 0.5f)) {
                    _state = State.FinishingFall;
                    _floorLerp.Restart(transform.position, _target);
                }
                else {
                    _state = State.Disabled;
                }
            }
        }

        private bool FoundFloor() {
            if (Physics.Raycast(transform.position, -transform.up,  out RaycastHit hit, _floorDistance + _offset, LayerMasks.Environment)) {
                _target = hit.point + (Vector3.up * _offset);
                return true;
            }
            return false;
        }

        private enum State {
            Disabled,
            FinishingFall,
            Throwing,
            Pulling,
            Falling
        }

        #if UNITY_EDITOR

        void OnDrawGizmos() {
            UnityEditor.Handles.Label(transform.position, _state.ToString());
            switch (_state) {
                case State.Falling:
                case State.Throwing:
                case State.FinishingFall:
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, _target);
                    break;
                case State.Pulling:
                    if (_pullTarget != null) {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(transform.position, _pullTarget.position);
                    }
                    break;
                
            }
        }
        #endif
    }
}
