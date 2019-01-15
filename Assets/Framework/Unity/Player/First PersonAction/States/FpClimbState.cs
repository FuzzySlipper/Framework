using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class FpClimbState : FpState {
        public Vector3 Pos {
            get { return _climbTarget; }
            set {
                _climbTarget = value;
                _positionSet = true;
            }
        }

        private Vector3 _climbTarget;
        private bool _positionSet = false;
        private bool _isClimbing = false;
        private float _startTime;
        private Vector3 _startPos;

        public override Labels Label { get { return FpState.Labels.Climb; } }
        public override float MoveSpeed { get { return Settings.WalkSpeed * Settings.ClimbingSpeed; } }
        public override bool VerticalMoving { get { return true; } }

        public override void Enter() {
            base.Enter();
            if (_positionSet) {
                StartClimb();
            }
        }

        private void StartClimb() {
            _isClimbing = true;
            _context.Rb.isKinematic = true;
            _context.Collider.enabled = false;
            _context.DisconnectAllGrapple();
            _startPos = _context.transform.position;
            _startTime = Time.time;
        }

        public override void UpdateMovement(Vector3 moveVector, bool isForward, ref Vector3 moveDirection) {
            if (!_isClimbing && _positionSet) {
                StartClimb();
            }
            if (!_positionSet) {
                return;
            }
            var percent = (Time.time - _startTime) / _context.Settings.ClimbTime;
            _context.transform.position = Vector3.Lerp(_startPos, _climbTarget, percent);
            if (percent >= 1) {
                _context.Machine.ChangeState<FpNormalState>();
            }
            //var horizontal = _context.MoveInput.x;//*Global.TimeScale;
            //var vertical = _context.MoveInput.y;//*Global.TimeScale; 

            //vertical *= isForward ? 1f : Settings.BackwardsSpeed;
            //horizontal *= Settings.SidewaysSpeed;

            //var screenMovementSpace = Quaternion.Euler(0f, Player.Camera.transform.eulerAngles.y, 0f);
            //var forwardVector = screenMovementSpace * Vector3.forward * vertical;
            //var rightVector = screenMovementSpace * Vector3.right * horizontal;

            //moveVector = forwardVector + rightVector;

            //var lookUp = Player.Camera.transform.forward.y > -.4f;
            //if (moveVector.z > 0) {
            //    //forwardVector = _currentLadder.transform.up*vertical;
            //    forwardVector *= lookUp ? 1f : -1f;
            //}
            //moveVector = forwardVector + rightVector;

            //if (FirstPersonController.Grounded) {
            //    if (isForward && !lookUp) {
            //        moveVector += screenMovementSpace * Vector3.forward;
            //    }
            //}
            //else {
            //    if (isForward && lookUp) {
            //        moveVector += screenMovementSpace * Vector3.forward;
            //    }
            //}
            //moveDirection = moveVector * MoveSpeed;

        }

        public override void Exit() {
            base.Exit();
            _positionSet = false;
            _isClimbing = false;
            _context.Rb.isKinematic = false;
            _context.Collider.enabled = true;
            _isClimbing = false;
        }
    }
}