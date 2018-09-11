using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class MoverSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<MoveTweenEvent>, IReceiveGlobal<StartMoveEvent> {
        private const float ReachedDestination = 0.1f;
        private const float ReachedDestinationSquared = ReachedDestination * ReachedDestination;

        private List<MoveTweenEvent> _moveTweens = new List<MoveTweenEvent>();
        private ManagedArray<RotateToTarget> _rotateList;
        private ManagedArray<RotateToTarget>.RunDel<RotateToTarget> _rotateDel;
        private ManagedArray<SimplerMover> _simpleMoveList;
        private ManagedArray<SimplerMover>.RunDel<SimplerMover> _simpleMoveDel;
        private ManagedArray<ArcMover> _arcMoverList;
        private ManagedArray<ArcMover>.RunDel<ArcMover> _arcMoveDel;

        public MoverSystem() {
            _rotateDel = HandleRotation;
            _simpleMoveDel = HandleMoveSimple;
            _arcMoveDel = HandleArcMovement;
        }

        public void OnSystemUpdate(float dt) {
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
            if (_rotateList == null) {
                _rotateList = EntityController.GetComponentArray<RotateToTarget>();
            }
            if (_rotateList != null) {
                _rotateList.Run(_rotateDel);
            }
            if (_simpleMoveList == null) {
                _simpleMoveList = EntityController.GetComponentArray<SimplerMover>();
            }
            if (_simpleMoveList != null) {
                _simpleMoveList.Run(_simpleMoveDel);
            }
            if (_arcMoverList == null) {
                _arcMoverList = EntityController.GetComponentArray<ArcMover>();
            }
            if (_arcMoverList != null) {
                _arcMoverList.Run(_arcMoveDel);
            }
        }
        
        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>(m => m.Clear());
        }

        private void RotateTowardsMoveTarget(Transform tr, Vector3 moveTarget, float speed) {
            var targetRotation = Quaternion.LookRotation(moveTarget - tr.position);
            tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRotation, speed * TimeManager.DeltaTime);
        }

        private void HandleMoveSimple(SimplerMover mover) {
            var entity = mover.GetEntity();
            if (!entity.Tags.Contain(EntityTags.Moving)) {
                return;
            }
            var target = entity.Get<MoveTarget>();
            if (target == null) {
                return;
            }
            var tr = entity.Get<TransformComponent>().Tr;
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

        private void HandleArcMovement(ArcMover mover) {
            var entity = mover.GetEntity();
            if (!entity.Tags.Contain(EntityTags.Moving)) {
                return;
            }
            mover.ElapsedTime += TimeManager.DeltaTime;
            mover.Transform.Tr.Translate(0, (mover.MoveVector.y - (mover.Speed * mover.ElapsedTime)) * TimeManager.DeltaTime, mover.MoveVector.z * TimeManager.DeltaTime);
            if (mover.ElapsedTime > mover.Duration) {
                FinishMove(entity, mover.Transform.Tr.position);
            }
        }

        private void HandleRotation(RotateToTarget r) {
            var targetRotation = Quaternion.LookRotation(r.Position - r.GetCurrent);
            if (r.Rb != null) {
                r.Rb.MoveRotation(Quaternion.RotateTowards(r.Rb.rotation, targetRotation, r.RotationSpeed * TimeManager.DeltaTime));
            }
            else {
                r.TargetTr.rotation = Quaternion.RotateTowards(r.TargetTr.rotation, targetRotation, r.RotationSpeed * TimeManager.DeltaTime);
            }
        }

        public void HandleGlobal(List<MoveTweenEvent> arg) {
            for (int i = 0; i < arg.Count; i++) {
                _moveTweens.Add(arg[i]);
                arg[i].Owner.Tags.Add(EntityTags.Moving);
            }
        }

        public void HandleGlobal(List<StartMoveEvent> arg) {
            for (int i = 0; i < arg.Count; i++) {
                var moveEvent = arg[i];
                moveEvent.Origin.Tags.Add(EntityTags.Moving);
                var target = moveEvent.Origin.Get<MoveTarget>();
                if (target == null) {
                    target = new MoveTarget(moveEvent.Origin);
                    moveEvent.Origin.Add(target);
                }
                target.TargetTr = moveEvent.Follow;
                target.TargetV3 = moveEvent.MoveTarget;
                moveEvent.Origin.Get<ArcMover>(m => CalculateFlight(m, moveEvent.GetPosition));
            }
        }

        private void CalculateFlight(ArcMover mover, Vector3 target) {
            float targetDistance = Vector3.Distance(mover.Transform.Tr.position, target);
            // Calculate the velocity needed to throw the object to the target at specified angle.
            float projectileVelocity = targetDistance / (Mathf.Sin(2 * mover.Angle * Mathf.Deg2Rad) / mover.Speed);
            mover.MoveVector.z = Mathf.Sqrt(projectileVelocity) * Mathf.Cos(mover.Angle * Mathf.Deg2Rad);
            mover.MoveVector.y = Mathf.Sqrt(projectileVelocity) * Mathf.Sin(mover.Angle * Mathf.Deg2Rad);
            // Calculate flight time.
            mover.Duration = targetDistance / mover.MoveVector.z;
            // Rotate projectile to face the target.
            mover.Transform.Tr.rotation = Quaternion.LookRotation(target - mover.Transform.Tr.position);
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
            Follow = follow.Tr.c;
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
}
