using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class MoverSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<MoveTweenEvent>, IReceiveGlobal<StartMoveEvent> {
        private const float ReachedDestination = 0.1f;
        private const float ReachedDestinationSquared = ReachedDestination * ReachedDestination;

        private List<MoveTweenEvent> _moveTweens = new List<MoveTweenEvent>();
        private List<ForwardMoverNode> _forwardMovers;
        private List<RotateToNode> _rotators;
        private List<SimpleMoverNode> _simpleMovers;
        private List<ArcMoverNode> _arcMovers;

        public MoverSystem() {
            NodeFilter<ForwardMoverNode>.New(ForwardMoverNode.GetTypes());
            NodeFilter<RotateToNode>.New(RotateToNode.GetTypes());
            NodeFilter<SimpleMoverNode>.New(SimpleMoverNode.GetTypes());
            NodeFilter<ArcMoverNode>.New(ArcMoverNode.GetTypes());
        }
        
        private UnscaledTimer _textTimer = new UnscaledTimer(0.5f);
        
        public void OnSystemUpdate(float dt, float unscaledDt) {
            for (int i = _moveTweens.Count - 1; i >= 0; i--) {
                _moveTweens[i].Tr.position = _moveTweens[i].Tween.Get();
                if (_moveTweens[i].Owner.Tags.Contain(EntityTags.RotateToMoveTarget)) {
                    RotateTowardsMoveTarget(_moveTweens[i].Tr, _moveTweens[i].Tween.EndValue, _moveTweens[i].Owner.Get<RotationSpeed>()?.Speed ?? 10);
                }
                if (!_moveTweens[i].Tween.Active) {
                    FinishMove(_moveTweens[i].Owner, _moveTweens[i].Tween.EndValue);
                    _moveTweens.RemoveAt(i);
                }
            }
            if (_forwardMovers == null) {
                _forwardMovers = EntityController.GetNodeList<ForwardMoverNode>();
            }
            if (_forwardMovers != null) {
                for (int i = 0; i < _forwardMovers.Count; i++) {
                    if (_forwardMovers[i].Entity.IsDestroyed()) {
                        continue;
                    }
                    HandleForwardMovement(_forwardMovers[i]);
                }
            }
            if (_rotators == null) {
                _rotators = EntityController.GetNodeList<RotateToNode>();
            }
            if (_rotators != null) {
                for (int i = 0; i < _rotators.Count; i++) {
                    if (_rotators[i].Entity.IsDestroyed()) {
                        continue;
                    }
                    HandleRotation(_rotators[i]);
                }
            }
            if (_simpleMovers == null) {
                _simpleMovers = EntityController.GetNodeList<SimpleMoverNode>();
            }
            if (_simpleMovers != null) {
                for (int i = 0; i < _simpleMovers.Count; i++) {
                    if (_simpleMovers[i].Entity.IsDestroyed()) {
                        continue;
                    }
                    HandleMoveSimple(_simpleMovers[i]);
                }
            }
            if (_arcMovers == null) {
                _arcMovers = EntityController.GetNodeList<ArcMoverNode>();
            }
            if (_arcMovers != null) {
                for (int i = 0; i < _arcMovers.Count; i++) {
                    if (_arcMovers[i].Entity.IsDestroyed()) {
                        continue;
                    }
                    HandleArcMovement(_arcMovers[i]);
                }
            }
        }
        
        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>()?.Complete();
        }

        private void RotateTowardsMoveTarget(Transform tr, Vector3 moveTarget, float speed) {
            var targetRotation = Quaternion.LookRotation(moveTarget - tr.position);
            tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRotation, speed * TimeManager.DeltaTime);
        }

        private void HandleMoveSimple(SimpleMoverNode mover) {
            var entity = mover.Entity;
            if (!entity.Tags.Contain(EntityTags.Moving)) {
                return;
            }
            var target = mover.MoveTarget;
            if (target == null) {
                return;
            }
            var tr = mover.Tr;
            if (tr == null) {
                return;
            }
            var targetPos = target.GetTargetPosition;
            var dir = targetPos - tr.position;
            tr.position = Vector3.MoveTowards(tr.position, targetPos, mover.MoveSpeed.Speed * TimeManager.DeltaTime);
            var targetRotation = Quaternion.LookRotation(dir);
            tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRotation, mover.RotationSpeed.Speed * TimeManager.DeltaTime);
            if (Vector3.Distance(targetPos, tr.position) < ReachedDestination) {
                FinishMove(entity, targetPos);
            }
        }

        //private void HandleForwardTargetMover(ForwardTargetMover mover) {
        //    var entity = mover.GetEntity();
        //    if (!entity.Tags.Contain(EntityTags.Moving)) {
        //        return;
        //    }
        //    var target = entity.Get<MoveTarget>();
        //    if (target == null) {
        //        return;
        //    }
        //    var tr = entity.Tr;
        //    if (tr == null) {
        //        return;
        //    }
        //    var targetPos = target.GetTargetPosition;
        //    var dir = targetPos - tr.position;
        //    tr.position = Vector3.MoveTowards(tr.position, targetPos, mover.Get<MoveSpeed>()?.Speed ?? 1 * TimeManager.DeltaTime);
        //    var targetRotation = Quaternion.LookRotation(dir);
        //    tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRotation, mover.Get<RotationSpeed>()?.Speed ?? 1 * TimeManager.DeltaTime);
        //    if (Vector3.Distance(targetPos, tr.position) < ReachedDestination) {
        //        FinishMove(entity, targetPos);
        //    }
        //}

        private void HandleArcMovement(ArcMoverNode mover) {
            var entity = mover.Entity;
            if (!entity.Tags.Contain(EntityTags.Moving)) {
                return;
            }
            mover.ArcMover.ElapsedTime += TimeManager.DeltaTime;
            mover.Tr.Translate(0, (mover.ArcMover.MoveVector.y - (mover.Get<MoveSpeed>()?.Speed ?? 1 * mover.ArcMover.ElapsedTime)) * TimeManager
            .DeltaTime, mover.ArcMover.MoveVector.z * TimeManager.DeltaTime);
            if (mover.ArcMover.ElapsedTime > mover.ArcMover.Duration) {
                FinishMove(entity, mover.Tr.position);
            }
        }

        private void HandleForwardMovement(ForwardMoverNode mover) {
            var entity = mover.Entity;
            if (!entity.Tags.Contain(EntityTags.Moving) || mover.Tr == null) {
                return;
            }
            var ms = mover.MoveSpeed?.Speed ?? 1;
            mover.Tr.Translate(Vector3.forward * ms * TimeManager.DeltaTime, Space.Self); 
        }

        private void HandleRotation(RotateToNode r) {
            var targetRotation = Quaternion.LookRotation(r.Rotate.Position - r.Tr.position);
            var rb = r.Rb?.Rb;
            if (rb != null) {
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, r.Rotate.RotationSpeed * TimeManager.DeltaTime));
            }
            else {
                r.Rotate.TargetTr.Tr.rotation = Quaternion.RotateTowards(r.Rotate.TargetTr.Tr.rotation, targetRotation, r.Rotate
                                                                                                                            .RotationSpeed * TimeManager
                .DeltaTime);
            }
        }

        public void HandleGlobal(MoveTweenEvent arg) {
            if (arg.Owner == null) {
                return;
            }
            _moveTweens.Add(arg);
            arg.Owner.Tags.Add(EntityTags.Moving);
        }

        public void HandleGlobal(StartMoveEvent moveEvent) {
            if (moveEvent.Origin == null) {
                return;
            }
            moveEvent.Origin.Tags.Add(EntityTags.Moving);
            var target = moveEvent.Origin.Get<MoveTarget>();
            if (target == null) {
                target = new MoveTarget();
                moveEvent.Origin.Add(target);
            }
            target.SetMoveTarget(moveEvent.GetPosition);
            CalculateFlight(moveEvent.Origin.Get<ArcMover>(), moveEvent.GetPosition, moveEvent.Origin.Get<MoveSpeed>());
        }

        private void CalculateFlight(ArcMover mover, Vector3 target, float speed) {
            var tr = mover.GetEntity().Get<TransformComponent>().Value;
            float targetDistance = Vector3.Distance(tr.position, target);
            // Calculate the velocity needed to throw the object to the target at specified angle.
            float projectileVelocity = targetDistance / (Mathf.Sin(2 * mover.Angle * Mathf.Deg2Rad) / speed);
            mover.MoveVector.z = Mathf.Sqrt(projectileVelocity) * Mathf.Cos(mover.Angle * Mathf.Deg2Rad);
            mover.MoveVector.y = Mathf.Sqrt(projectileVelocity) * Mathf.Sin(mover.Angle * Mathf.Deg2Rad);
            // Calculate flight time.
            mover.Duration = targetDistance / mover.MoveVector.z;
            // Rotate projectile to face the target.
            tr.rotation = Quaternion.LookRotation(target - tr.position);
            mover.ElapsedTime = 0;
        }
    }

    public struct StartMoveEvent : IEntityMessage {
        public Vector3 MoveTarget;
        public Transform Follow;
        public Entity Origin;

        public Vector3 GetPosition => Follow != null ? Follow.position : MoveTarget;

        public StartMoveEvent(Entity origin, Vector3 moveTarget, Transform follow) {
            MoveTarget = moveTarget;
            Follow = follow;
            Origin = origin;
        }

        public StartMoveEvent(Entity origin, VisibleNode follow) {
            MoveTarget = follow.position;
            Follow = follow.Tr;
            Origin = origin;
        }
    }

    public struct MoveTweenEvent : IEntityMessage {
        public TweenV3 Tween;
        public Transform Tr;
        public Entity Owner;

        public MoveTweenEvent(TweenV3 tween, Transform tr, Entity owner) {
            Tween = tween;
            Tr = tr;
            Owner = owner;
        }

        public MoveTweenEvent(Vector3 target, Transform tr, Entity owner) {
            var distance = Vector3.Distance(tr.position, target);
            var duration = distance / MathEx.Max(owner.GetMoveSpeed(), 1);
            Tween = new TweenV3(tr.position, target, duration, EasingTypes.SinusoidalInOut);
            Tr = tr;
            Owner = owner;
        }
    }

    public struct MoveComplete : IEntityMessage {
        public int Target;
        public Vector3 MoveTarget;

        public MoveComplete(int target, Vector3 moveTarget) {
            Target = target;
            MoveTarget = moveTarget;
        }
    }

    public struct AddForceEvent : IEntityMessage {
        public Rigidbody Rb;
        public Vector3 Force;

        public AddForceEvent(Rigidbody rb, Vector3 force) {
            Rb = rb;
            Force = force;
        }

        //def force = 500f
        public AddForceEvent(Rigidbody rb, Vector3 moveTarget, float force) {
            Rb = rb;
            Force = (moveTarget - Rb.position).normalized * force;
        }
    }
    public class ForwardMoverNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<ForwardMover> _forward = new CachedComponent<ForwardMover>();
        private CachedComponent<MoveSpeed> _moveSpeed = new CachedComponent<MoveSpeed>();

        public Transform Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public ForwardMover ForwardMover { get => _forward; }
        public MoveSpeed MoveSpeed { get => _moveSpeed; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _forward, _moveSpeed
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(ForwardMover),
            };
        }
    }

    public class RotateToNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<RotateToTarget> _rotate = new CachedComponent<RotateToTarget>();
        private CachedComponent<RigidbodyComponent> _rb = new CachedComponent<RigidbodyComponent>();

        public Transform Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public RotateToTarget Rotate { get => _rotate; }
        public RigidbodyComponent Rb { get => _rb; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider,  _rotate, _rb
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(RotateToTarget),
            };
        }
    }

    public class ArcMoverNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<ArcMover> _arcMover = new CachedComponent<ArcMover>();
        private CachedComponent<MoveSpeed> _moveSpeed = new CachedComponent<MoveSpeed>();
        
        public Transform Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public ArcMover ArcMover { get => _arcMover; }
        public MoveSpeed MoveSpeed { get => _moveSpeed; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _arcMover,_moveSpeed
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(ArcMover),
            };
        }
    }

    public class SimpleMoverNode : BaseNode {

        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<ColliderComponent> _collider = new CachedComponent<ColliderComponent>();
        private CachedComponent<SimplerMover> _simple = new CachedComponent<SimplerMover>();
        private CachedComponent<MoveSpeed> _moveSpeed = new CachedComponent<MoveSpeed>();
        private CachedComponent<MoveTarget> _moveTarget = new CachedComponent<MoveTarget>();
        private CachedComponent<RotationSpeed> _rotationSpeed = new CachedComponent<RotationSpeed>();
        
        public Transform Tr { get => _tr.Value; }
        public Collider Collider { get => _collider.Value.Collider; }
        public SimplerMover Simple { get => _simple; }
        public MoveSpeed MoveSpeed { get => _moveSpeed; }
        public RotationSpeed RotationSpeed { get => _rotationSpeed; }
        public MoveTarget MoveTarget { get => _moveTarget; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _tr, _collider, _simple, _moveSpeed, _moveTarget, _rotationSpeed
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(TransformComponent),
                typeof(SimplerMover),
            };
        }
    }
}
