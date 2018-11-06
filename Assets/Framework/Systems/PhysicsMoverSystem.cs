using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PhysicsMoverSystem : SystemBase, IMainFixedUpdate, IReceiveGlobal<AddForceEvent> {

        private const float ReachedDestination = 0.1f;
        
        private bool _frozen = false;
        private List<RigidbodyMoverNode> _moverList;
        private ManagedArray<RigidbodyComponent> _rbList;

        public PhysicsMoverSystem() {
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
        }

        public override void Dispose() {
            base.Dispose();
            MessageKit.removeObserver(Messages.PauseChanged, CheckPause);
        }

        public void OnFixedSystemUpdate(float dt) {
            if (_moverList == null) {
                _moverList = EntityController.GetNodeList<RigidbodyMoverNode>();
            }
            if (_moverList != null) {
                for (int i = 0; i < _moverList.Count; i++) {
                    HandleVelocityMover(_moverList[i]);
                }
            }
            if (_frozen) {
                CheckRigidbodies();
            }
        }

        private void CheckPause() {
            if (_frozen == Game.Paused) {
                return;
            }
            _frozen = Game.Paused;
            CheckRigidbodies();
        }

        private void CheckRigidbodies() {
            if (_rbList == null) {
                _rbList= EntityController.GetComponentArray<RigidbodyComponent>();
            }
            if (_rbList != null) {
                _rbList.Run(CheckPaused);
            }
        }

        private void CheckPaused(RigidbodyComponent component) {
            if (component.RigidbodySetup.IsFrozen == _frozen) {
                return;
            }
            if (_frozen) {
                component.RigidbodySetup.Freeze();
            }
            else {
                component.RigidbodySetup.Restore();
            }
        }

        private void HandleVelocityMover(RigidbodyMoverNode mover) {
            var dt = TimeManager.DeltaTime;
            var rb = mover.Rb.c.Rb;
            if (rb == null) {
                return;
            }
            var moveSpeed = mover.MoveSpeed.c;
            mover.Mover.c.CurrentSpeed = Mathf.MoveTowards(mover.Mover.c.CurrentSpeed, moveSpeed , moveSpeed * 0.25f * dt);
            var moveTarget = mover.Target.c.GetTargetPosition;
            var dir = moveTarget - rb.position;
            rb.AddForce(dir.normalized * mover.Mover.c.CurrentSpeed * dt);
            var targetRotation = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(mover.Tr.c.rotation, targetRotation, mover.RotationSpeed.c.Speed * dt));
            if (Vector3.Distance(moveTarget, rb.position) < ReachedDestination) {
                FinishMove(mover.Entity, moveTarget);
            }
        }

        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>(m => m.Clear());
        }

        public void HandleGlobal(ManagedArray<AddForceEvent> arg) {
            for (int i = 0; i < arg.Count; i++) {
                arg[i].Rb.AddForce(arg[i].Force);
            }
        }
    }
}
