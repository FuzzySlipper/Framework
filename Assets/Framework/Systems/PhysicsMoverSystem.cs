using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PhysicsMoverSystem : SystemBase, IMainFixedUpdate, IReceiveGlobal<AddForceEvent> {

        private const float ReachedDestination = 0.1f;

        private ManagedArray<VelocityMover> _list;
        private ManagedArray<VelocityMover>.RunDel<VelocityMover> _del;

        public void OnFixedSystemUpdate() {
            if (_list == null) {
                _del = HandleVelocityMover;
                _list = EntityController.GetComponentArray<VelocityMover>();
            }
            if (_list != null) {
                _list.Run(_del);
            }
        }

        private void HandleVelocityMover(VelocityMover mover) {
            var dt = TimeManager.DeltaTime;
            mover.CurrentSpeed = Mathf.MoveTowards(mover.CurrentSpeed, mover.Speed, mover.Acceleration * dt);
            var moveTarget = mover.Target.GetTargetPosition;
            var dir = moveTarget - mover.Rigidbody.Rb.position;
            mover.Rigidbody.Rb.AddForce(dir.normalized * mover.Speed * dt);
            var targetRotation = Quaternion.LookRotation(dir);
            mover.Rigidbody.Rb.MoveRotation(Quaternion.RotateTowards(mover.Transform.Tr.rotation, targetRotation, mover.Rotation * dt));
            if (Vector3.Distance(moveTarget, mover.Rigidbody.Rb.position) < ReachedDestination) {
                FinishMove(mover.GetEntity(), moveTarget);
            }
        }

        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>(m => m.Clear());
        }

        public void HandleGlobal(List<AddForceEvent> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Rb.AddForce(arg[i].Force);
            }
        }
    }
}
