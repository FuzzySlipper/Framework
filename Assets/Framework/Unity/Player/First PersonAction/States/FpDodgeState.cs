using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class FpDodgeState : FpState {

        private Vector3 _dodgeVector;
        private TweenFloat _dodgeTween;
        private bool _addedVector = false;

        public override Labels Label { get { return FpState.Labels.Dodge; } }
        public override float MoveSpeed { get { return Settings.WalkSpeed * Settings.InAirSpeed; } }
        public override bool VerticalMoving { get { return true; } }

        public override void Enter() {
            _addedVector = false;
        }

        public override void UpdateMovement(Vector3 moveVector, bool isForward, ref Vector3 moveDirection) {
            if (!_addedVector) {
                SurfaceDetector.main.PlayJumpingSound(_context);
                if (moveVector.magnitude < 0.15f) {
                    var screenMovementSpace = Quaternion.Euler(0f, Player.Cam.transform.eulerAngles.y, 0f);
                    var forwardVector = screenMovementSpace * Vector3.forward * -2;
                    var rightVector = screenMovementSpace * Vector3.right * 0;
                    moveDirection = forwardVector + rightVector;
                }
                _dodgeVector = moveDirection * Settings.DodgeForce;
                _dodgeVector.y = Settings.DodgeForce * 0.5f;
                _dodgeTween = new TweenFloat(0, 1, Settings.DodgeLength, Settings.DodgeEase, true);
                _dodgeTween.Init();
                _addedVector = true;
            }
            moveDirection = _dodgeVector * _dodgeTween.Get();
            //moveDirection = FallVector(moveDirection);
            if (!_dodgeTween.Active) {
                _machine.ChangeState<FpNormalState>();
            }
        }
    }
}