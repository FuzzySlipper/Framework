using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public abstract class FpState : MachineState<FirstPersonController> {
        protected static RaycastHit FloorHit;

        private bool _initialized = false;

        public override void Enter() {
            if (!_initialized) {
                _initialized = true;
                OnInit();
            }
        }

        public abstract Labels Label { get; }
        public override string Description { get { return EnumHelper.GetString<Labels>((int) Label); } }

        public virtual float MoveSpeed {
            get {
                //FirstPersonController.Running = _context.InputRun && !Player.Controller.Slowed;
                //var speed = FirstPersonController.Running ? Settings.RunSpeed : Settings.WalkSpeed;
                //return speed * (Player.Controller.Slowed ? Settings.SlowSpeedModifier : 1);
                return Settings.WalkSpeed;
            }
        }

        public virtual bool VerticalMoving { get { return false; } }

        public virtual void Stop() {
        }

        public override void Update(float deltaTime) {
        }

        public virtual void UpdateMovement(Vector3 moveVector, bool isForward, ref Vector3 moveDirection) {
            if (!FirstPersonController.Grounded) {
                //moveDirection = FallVector(moveDirection);
                _machine.ChangeState<FpFallState>();
                return;
            }
            moveDirection = moveVector * MoveSpeed;
            //moveDirection.y = -10f;
            Physics.SphereCast(_context.transform.position + _context.Center, _context.Radius, Vector3.down,
                out FloorHit, _context.Height * .5f);
            _context.CurrentSurface = SurfaceDetector.main.GetSurface(FloorHit);
        }

        protected virtual void OnInit() {
        }

        protected FpControllerSettings Settings { get { return _context.Settings; } }

        //protected Vector3 FallVector(Vector3 moveDirection) {
        //    moveDirection += Physics.gravity * Settings.GravityMultiplier * Global.FixedDelta;
        //    return moveDirection;
        //}

        public static FpState GetState(Labels label) {
            switch (label) {
                default:
                case Labels.Normal:
                    return new FpNormalState();
                case Labels.Fall:
                    return new FpFallState();
                case Labels.Jump:
                    return new FpJumpState();
                case Labels.Dodge:
                    return new FpDodgeState();
                case Labels.Crouch:
                    return new FpCrouchState();
                case Labels.Climb:
                    return new FpClimbState();
            }
        }

        public enum Labels {
            Normal = 0,
            Fall,
            Jump,
            Dodge,
            Crouch,
            Climb
        }
    }
}