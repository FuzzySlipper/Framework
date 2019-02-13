using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class FpJumpState : FpState {

        //private bool _addedJump = false;
        private ScaledTimer _checkJumpTime = new ScaledTimer(0.15f);
        private float MaxJumpVelocity { get { return Settings.JumpForce * 1.1f; } }

        public override Labels Label { get { return FpState.Labels.Jump; } }
        public override float MoveSpeed { get { return Settings.WalkSpeed * Settings.InAirSpeed; } }
        public override bool VerticalMoving { get { return true; } }

        public override void Enter() {
            //_addedJump = false;
            ProcessJump();
        }

        private void ProcessJump() {
            SurfaceDetector.main.PlayJumpingSound(_context);
            _context.Rb.drag = 0f;
            var force = Settings.JumpForce - _context.Rb.velocity.y;
            if (force > 0) {
                _context.Rb.AddForce(new Vector3(0f, force, 0f), ForceMode.VelocityChange);
            }
            _checkJumpTime.Activate();
        }

        public override void UpdateMovement(Vector3 moveVector, bool isForward, ref Vector3 moveDirection) {
            //if (!_addedJump) {
            //    //moveDirection.y = Settings.JumpForce;
            //    ProcessJump();
            //    _addedJump = true;
            //    
            //}
            if (_checkJumpTime.IsActive) {
                //if (_context.Rb.velocity.y > MaxJumpVelocity) {
                //    Debug.DrawRay(_context.transform.position, -_context.transform.up, Color.yellow, 5f);
                //    _context.Rb.AddForce(new Vector3(0f, -(_context.Rb.velocity.y - MaxJumpVelocity), 0f), ForceMode.VelocityChange);
                //}
            }
            else if (FirstPersonController.Grounded) {
                _machine.ChangeState<FpNormalState>();
            }
            else {
                _machine.ChangeState<FpFallState>();
            }
        }
    }
}