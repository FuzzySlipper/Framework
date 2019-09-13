using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PhysicsMoverSystem : SystemBase, IMainFixedUpdate, IReceiveGlobal<AddForceEvent> {

        private const float ReachedDestination = 0.1f;
        
        private bool _frozen = false;
        private List<RigidbodyMoverNode> _moverList;
        private ManagedArray<RigidbodyComponent> _rbList;
        private ManagedArray<RigidbodyComponent>.Delegate _rbDel;

        public PhysicsMoverSystem() {
            _rbDel = CheckPaused;
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
                _rbList.Run(_rbDel);
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
            var rb = mover.Rb.Value.Rb;
            if (rb == null) {
                return;
            }
            var moveSpeed = mover.MoveSpeed.Value;
            mover.Mover.Value.CurrentSpeed = Mathf.MoveTowards(mover.Mover.Value.CurrentSpeed, moveSpeed , moveSpeed * 0.25f * dt);
            var moveTarget = mover.Target.Value.GetTargetPosition;
            var dir = moveTarget - rb.position;
            rb.AddForce(dir.normalized * mover.Mover.Value.CurrentSpeed * dt);
            var targetRotation = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(mover.Entity.Tr.rotation, targetRotation, mover.RotationSpeed.Value.Speed * dt));
            if (Vector3.Distance(moveTarget, rb.position) < ReachedDestination) {
                FinishMove(mover.Entity, moveTarget);
            }
        }

        private void FinishMove(Entity owner, Vector3 moveTarget) {
            owner.Tags.Remove(EntityTags.Moving);
            owner.Post(new MoveComplete(owner, moveTarget));
            owner.Get<MoveTarget>(m => m.Complete());
        }

        public void HandleGlobal(AddForceEvent arg) {
            arg.Rb.AddForce(arg.Force);
        }
    }
}
