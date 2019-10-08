using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class MoverSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<MoveTweenEvent>, IReceiveGlobal<StartMoveEvent> {
        private const float ReachedDestination = 0.1f;
        private const float ReachedDestinationSquared = ReachedDestination * ReachedDestination;

        private List<MoveTweenEvent> _moveTweens = new List<MoveTweenEvent>();
        private NodeList<ForwardMoverNode> _forwardMovers;
        private NodeList<RotateToNode> _rotators;
        private NodeList<SimpleMoverNode> _simpleMovers;
        private NodeList<ArcMoverNode> _arcMovers;

        private ManagedArray<ForwardMoverNode>.RefDelegate _forwardDel;
        private ManagedArray<RotateToNode>.RefDelegate _rotateDel;
        private ManagedArray<SimpleMoverNode>.RefDelegate _simpleDel;
        private ManagedArray<ArcMoverNode>.RefDelegate _arcDel;

        public MoverSystem() {
            NodeFilter<ForwardMoverNode>.Setup(ForwardMoverNode.GetTypes());
            _forwardMovers = EntityController.GetNodeList<ForwardMoverNode>();
            _forwardDel = HandleForwardMovement;
            NodeFilter<RotateToNode>.Setup(RotateToNode.GetTypes());
            _rotators = EntityController.GetNodeList<RotateToNode>();
            _rotateDel = HandleRotation;
            NodeFilter<SimpleMoverNode>.Setup(SimpleMoverNode.GetTypes());
            _simpleMovers = EntityController.GetNodeList<SimpleMoverNode>();
            _simpleDel = HandleMoveSimple;
            NodeFilter<ArcMoverNode>.Setup(ArcMoverNode.GetTypes());
            _arcMovers = EntityController.GetNodeList<ArcMoverNode>();
            _arcDel = HandleArcMovement;
        }
        
        public void OnSystemUpdate(float dt, float unscaledDt) {
            for (int i = _moveTweens.Count - 1; i >= 0; i--) {
                _moveTweens[i].Entity.Post(new SetTransformPosition(_moveTweens[i].Tr, _moveTweens[i].Tween.Get()));
                if (_moveTweens[i].Entity.Tags.Contain(EntityTags.RotateToMoveTarget)) {
                    var speed = _moveTweens[i].Entity.Get<RotationSpeed>()?.Speed ?? 10;
                    var targetRotation = Quaternion.LookRotation(_moveTweens[i].Tween.EndValue - _moveTweens[i].Tr.position);
                    var rotation = Quaternion.RotateTowards(_moveTweens[i].Tr.rotation, targetRotation, speed * TimeManager.DeltaTime);
                    _moveTweens[i].Entity.Post(new SetTransformRotation(_moveTweens[i].Tr, rotation));
                    
                }
                if (!_moveTweens[i].Tween.Active) {
                    FinishMove(_moveTweens[i].Entity, _moveTweens[i].Tween.EndValue);
                    _moveTweens.RemoveAt(i);
                }
            }
            _forwardMovers.Run(_forwardDel);
            _rotators.Run(_rotateDel);
            _simpleMovers.Run(_simpleDel);
            _arcMovers.Run(_arcDel);
        }
        
        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>()?.Complete();
        }

        private void HandleMoveSimple(ref SimpleMoverNode mover) {
            var entity = mover.Entity;
            if (entity.IsDestroyed() || !entity.Tags.Contain(EntityTags.Moving)) {
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
            mover.Entity.Post(new SetTransformPosition(mover.Tr,
                Vector3.MoveTowards(tr.position, targetPos, mover.MoveSpeed.Speed * TimeManager.DeltaTime)));
            var targetRotation = Quaternion.LookRotation(dir);
            mover.Entity.Post(new SetTransformRotation(mover.Tr, Quaternion.RotateTowards(tr.rotation, targetRotation, mover.RotationSpeed
            .Speed * TimeManager.DeltaTime)));
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

        private void HandleArcMovement(ref ArcMoverNode mover) {
            var entity = mover.Entity;
            if (entity.IsDestroyed() || !entity.Tags.Contain(EntityTags.Moving)) {
                return;
            }
            mover.ArcMover.ElapsedTime += TimeManager.DeltaTime;
            var dir = new Vector3(
                0, (mover.ArcMover.MoveVector.y - (mover.Get<MoveSpeed>()?.Speed ?? 1 * mover.ArcMover.ElapsedTime)) * TimeManager
                       .DeltaTime, mover.ArcMover.MoveVector.z * TimeManager.DeltaTime);
            mover.Entity.Post(new MoveTransform(mover.Tr, dir));
            if (mover.ArcMover.ElapsedTime > mover.ArcMover.Duration) {
                FinishMove(entity, mover.Tr.position);
            }
        }

        private void HandleForwardMovement(ref ForwardMoverNode mover) {
            var entity = mover.Entity;
            if (!entity.Tags.Contain(EntityTags.Moving) || mover.Tr == null) {
                return;
            }
            var ms = mover.MoveSpeed?.Speed ?? 1;
            mover.Entity.Post(new MoveTransform(mover.Tr, mover.Tr.forward * ms * TimeManager.DeltaTime));
        }

        private void HandleRotation(ref RotateToNode r) {
            if (r.Entity.IsDestroyed()) {
                return;
            }
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
            if (arg.Entity == null) {
                return;
            }
            _moveTweens.Add(arg);
            arg.Entity.Tags.Add(EntityTags.Moving);
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
            var tr = mover.GetEntity().Get<TransformComponent>();
            float targetDistance = Vector3.Distance(tr.position, target);
            // Calculate the velocity needed to throw the object to the target at specified angle.
            float projectileVelocity = targetDistance / (Mathf.Sin(2 * mover.Angle * Mathf.Deg2Rad) / speed);
            mover.MoveVector.z = Mathf.Sqrt(projectileVelocity) * Mathf.Cos(mover.Angle * Mathf.Deg2Rad);
            mover.MoveVector.y = Mathf.Sqrt(projectileVelocity) * Mathf.Sin(mover.Angle * Mathf.Deg2Rad);
            // Calculate flight time.
            mover.Duration = targetDistance / mover.MoveVector.z;
            // Rotate projectile to face the target.
            var entity = mover.GetEntity();
            entity.Post(new SetTransformRotation( entity.Get<TransformComponent>(), Quaternion.LookRotation(target - tr.position)));
            mover.ElapsedTime = 0;
        }
    }

    public struct StartMoveEvent : IEntityMessage {
        public Vector3 MoveTarget;
        public TransformComponent Follow;
        public Entity Origin;

        public Vector3 GetPosition => Follow != null ? Follow.position : MoveTarget;

        public StartMoveEvent(Entity origin, Vector3 moveTarget, TransformComponent follow) {
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
        public TweenV3 Tween { get; }
        public TransformComponent Tr { get; }
        public Entity Entity { get; }

        public MoveTweenEvent(TweenV3 tween, TransformComponent tr, Entity entity) {
            Tween = tween;
            Tr = tr;
            Entity = entity;
        }

        public MoveTweenEvent(Vector3 target, TransformComponent tr, Entity entity) {
            var distance = Vector3.Distance(tr.position, target);
            var duration = distance / MathEx.Max(entity.GetMoveSpeed(), 1);
            Tween = new TweenV3(tr.position, target, duration, EasingTypes.SinusoidalInOut);
            Tr = tr;
            Entity = entity;
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

        public TransformComponent Tr { get => _tr.Value; }
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

        public TransformComponent Tr { get => _tr.Value; }
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
        
        public TransformComponent Tr { get => _tr.Value; }
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
        
        public TransformComponent Tr { get => _tr.Value; }
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
