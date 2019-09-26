using System.Collections;
using UnityEngine;

namespace PixelComrades {
    public class GridCamera : MonoSingleton<GridCamera> {

        private enum Status {
            Inactive,
            InRotation,
            Rotated,
            LookingDown,
        }

        [SerializeField] private int _xMinLimit = -80;
        [SerializeField] private int _yMaxLimit = 70;
        [SerializeField] private int _yMinLimit = -70;
        [SerializeField] private float _zoomDampening = 10f;
        [SerializeField] private int _xMaxLimit = 80;
        [SerializeField] private TweenQuaternion _resetTween = new TweenQuaternion();
        [SerializeField] private bool _resetOnViewFinish = true;
        [SerializeField] private Vector3 _lookDownTarget = new Vector3(25,0,0);

        private Quaternion _desiredRotation;
        private Quaternion _startingRotation;
        private float _xDeg;
        private float _yDeg = 2.5f;
        private Status _currentStatus = Status.Inactive;
        private Task _resetView;
        private float _lookSensitivity;

        private bool ShouldCancelRotation {
            get {
                return Input.GetMouseButtonUp(1);
            }
        }

        private bool ShouldStartMouseLook {
            get {
                return Input.GetMouseButtonDown(1) && !PlayerInput.IsCursorOverUI && UICenterTarget.CurrentCharacter == null;
            }
        }

        void Awake() {
            _startingRotation = transform.localRotation;
            MessageKit.addObserver(Messages.PlayerMoving, ResetView);
            MessageKit.addObserver(Messages.PlayerRotated, ResetView);
            MessageKit.addObserver(Messages.OptionsChanged, SetOptions);
        }

        private void SetOptions() {
            _lookSensitivity = GameOptions.Get("LookSensitivity", 2.5f);
        }

        private void ResetView() {
            if (_currentStatus == Status.Inactive || _resetView != null || GameOptions.MouseLook) {
                return;
            }
            _resetView = TimeManager.StartUnscaled(ChangeViewLerp(_startingRotation, Status.Inactive));
        }

        private IEnumerator ChangeViewLerp(Quaternion target, Status endStatus) {
            _resetTween.Restart(transform.localRotation, target);
            while (_resetTween.Active) {
                transform.localRotation = _resetTween.Get();
                yield return null;
            }
            _currentStatus = endStatus;
            _resetView = null;
        }

        public void LookRotation() {
            if (ShouldStartMouseLook) {
                if (_resetView != null) {
                    TimeManager.Cancel(_resetView);
                    _resetView = null;
                }
                _xDeg = 0.0f;
                _yDeg = 2.5f;
                _currentStatus = Status.InRotation;
            }
            if (_currentStatus != Status.InRotation) {
                return;
            }
            Cursor.visible = false;
            FreeLook();
            if (ShouldCancelRotation) {
                Cursor.visible = true;
                _currentStatus = Status.Rotated;
                if (_resetOnViewFinish) {
                    ResetView();
                }
            }
        }

        public void LookDown() {
            if (_resetView != null) {
                TimeManager.Cancel(_resetView);
                _resetView = null;
            }
            if (_currentStatus == Status.LookingDown) {
                ResetView();
                return;
            }
            var target = Quaternion.Euler(_lookDownTarget);
            _resetView = TimeManager.StartUnscaled(ChangeViewLerp(target, Status.LookingDown));
        }

        private void FreeLook() {
            //_xDeg += (Input.GetAxis("Mouse X")*_xSpeed*0.02f);
            //_yDeg -= (Input.GetAxis("Mouse Y")*_ySpeed*0.02f);
            _xDeg += (PlayerInput.LookInput.x * _lookSensitivity);
            _yDeg -= (PlayerInput.LookInput.y * _lookSensitivity);
            _xDeg = ClampAngle(_xDeg, _xMinLimit, _xMaxLimit);
            _yDeg = ClampAngle(_yDeg, _yMinLimit, _yMaxLimit);
            _desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0.0f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _desiredRotation, TimeManager.DeltaTime *_zoomDampening);;
        }

        private static float ClampAngle(float angle, float min, float max) {
            if (angle < -360.0) {
                angle += 360f;
            }
            if (angle > 360.0) {
                angle -= 360f;
            }
            return Mathf.Clamp(angle, min, max);
        }
    }
}