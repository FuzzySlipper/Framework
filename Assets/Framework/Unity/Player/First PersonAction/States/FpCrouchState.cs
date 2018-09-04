using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class FpCrouchState : FpState {
        public override Labels Label { get { return FpState.Labels.Crouch; } }

        private float _nativeCapsuleHeight, _crouchVel;
        private Vector3 _crouchVelVec;
        private bool _inMotion = false;

        protected override void OnInit() {
            _nativeCapsuleHeight = _context.Height;
        }

        public override void Enter() {
            TimeManager.Start(SitDown());
        }

        public override void Stop() {
            if (_inMotion) {
                return;
            }
            if (Physics.SphereCast(_context.transform.position + Vector3.up * .75f, _context.Radius, Vector3.up,
                out FloorHit, _nativeCapsuleHeight * .25f)) {
                return;
            }
            TimeManager.Start(StandUp());
        }

        public override float MoveSpeed { get { return Settings.WalkSpeed * Settings.SlowSpeedModifier; } }

        public override void Exit() {
            _context.Height = _nativeCapsuleHeight;
            _context.Center = Vector3.up;
        }

        private IEnumerator SitDown() {
            var targetCenter = Vector3.up * (Settings.CrouchHeight * .5f);
            _inMotion = true;
            while (PlayCrouchAnimation(ref targetCenter, ref Settings.CrouchHeight)) {
                yield return null;
            }
            _inMotion = false;
            _context.Height = Settings.CrouchHeight;
            _context.Center = targetCenter;
        }

        private IEnumerator StandUp() {
            var targetCenter = Vector3.up;
            _inMotion = true;
            while (PlayCrouchAnimation(ref targetCenter, ref _nativeCapsuleHeight)) {
                yield return null;
            }
            _inMotion = false;
            _machine.ChangeState<FpNormalState>();
        }

        private bool PlayCrouchAnimation(ref Vector3 targetCenter, ref float targetHeight) {
            _context.Height = Mathf.SmoothDamp(_context.Height, targetHeight, ref _crouchVel,
                TimeManager.FixedDelta * 5f);
            _context.Center = Vector3.SmoothDamp(_context.Center, targetCenter, ref _crouchVelVec,
                TimeManager.FixedDelta * 5f);

            const int digits = 3;
            var cMag = System.Math.Round(_context.Center.magnitude, digits);
            var tMag = System.Math.Round(targetCenter.magnitude, digits);
            return cMag != tMag;
        }
    }
}