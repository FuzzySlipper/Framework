using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PhysicsMoverSystem : SystemBase, IMainFixedUpdate, IReceiveGlobal<AddForceEvent> {

        private const float ReachedDestination = 0.1f;
        
        private bool _frozen = false;
        private TemplateList<RigidbodyMoverTemplate> _moverList;
        private ManagedArray<RigidbodyMoverTemplate>.RefDelegate _moverDel;
        private ManagedArray<RigidbodyComponent> _rbList;
        private ManagedArray<RigidbodyComponent>.RefDelegate _rbDel;

        public PhysicsMoverSystem() {
            _rbDel = CheckPaused;
            _moverDel = HandleVelocityMover;
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
            _moverList = EntityController.GetTemplateList<RigidbodyMoverTemplate>();
        }

        public override void Dispose() {
            base.Dispose();
            MessageKit.removeObserver(Messages.PauseChanged, CheckPause);
        }

        public void OnFixedSystemUpdate(float dt) {
            if (_moverList != null) {
                _moverList.Run(_moverDel);
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
                _rbList.Run(_rbDel);
            }
        }

        private void CheckPaused(ref RigidbodyComponent component) {
            if (component.RigidbodySetup.IsFrozen == _frozen || component.Rb == null) {
                return;
            }
            if (_frozen) {
                component.RigidbodySetup.Freeze();
            }
            else {
                component.RigidbodySetup.Restore();
            }
        }

        private void HandleVelocityMover(ref RigidbodyMoverTemplate mover) {
            var dt = TimeManager.DeltaTime;
            var rb = mover.Rb;
            if (rb == null) {
                return;
            }
            var moveSpeed = mover.MoveSpeed;
            mover.VelocityMover.CurrentSpeed = Mathf.MoveTowards(mover.VelocityMover.CurrentSpeed, moveSpeed , moveSpeed * 0.25f * dt);
            var moveTarget = mover.Target.GetTargetPosition;
            var dir = moveTarget - rb.position;
            rb.AddForce(dir.normalized * mover.VelocityMover.CurrentSpeed * dt);
            var targetRotation = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(mover.Tr.rotation, targetRotation, mover.RotationSpeed.Speed * dt));
            if (Vector3.Distance(moveTarget, rb.position) < ReachedDestination) {
                FinishMove(mover.Entity, moveTarget);
            }
        }

        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>()?.Complete();
        }

        public void HandleGlobal(AddForceEvent arg) {
            arg.Rb.AddForce(arg.Force);
        }
    }
}
