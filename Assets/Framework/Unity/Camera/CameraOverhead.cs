using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public class CameraOverhead : MonoBehaviour {

        [SerializeField] private Camera _cam = null;
        [SerializeField] private Renderer _background = null;
        [SerializeField] private Vector3 _moveSpeed = new Vector3(30, 30, 30);
        [SerializeField] private bool _clampPos = false;
        [SerializeField] private Vector3 _worldLowerLeft;
        [SerializeField] private Vector3 _worldUpperRight;
        [SerializeField] private bool _scanForWorldLimit = false;
        [SerializeField] private FloatRange _scrollLimit = new FloatRange(40, 90);
        [SerializeField] private float _scrollSpeed = 50;
        [SerializeField] private bool _autoUpdate = false;
        [SerializeField] private int _axis = 1;

        private Transform _camTr;

        public Camera Cam { get { return _cam; } }

        void Awake() {
            _camTr = _cam.transform;
            
        }

        void Update() {
            if (_autoUpdate) {
                var input = new Vector3(PlayerInputSystem.LookInput.x, 0, 0);
                input[_axis] = PlayerInputSystem.LookInput.y;
                UpdateDrag(input);
                Zoom(Mouse.current.scroll.ReadValue().y);
            }
        }

        public void UpdateDrag(Vector3 moveDelta) {
            _camTr.Translate(moveDelta.Multiply(_moveSpeed), UnityEngine.Space.World);
            if (_clampPos) {
                Refresh();
            }
        }

        public void SetManualBounds(Bounds bounds, Vector3 center, Transform tr) {
            _scanForWorldLimit = false;
            SetBounds(bounds, center, tr);
        }

        public void ResetAutoBounds() {
            _scanForWorldLimit = true;
            SetBounds(_background.bounds, _background.transform.position, _background.transform);
        }

        private void SetBounds(Bounds bounds, Vector3 v3Center, Transform boundsTr) {
            Vector3 v3Ext = bounds.extents * 1.1f;
            //_worldUpperRight = boundsTr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y - v3ext.y, _cam.transform.position.z));
            //_worldLowerLeft = boundsTr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y + v3ext.y, _cam.transform.position.z));
            _worldUpperRight = new Vector3(v3Center.x + v3Ext.x, v3Center.y + v3Ext.y, boundsTr.position.z);
            _worldLowerLeft = new Vector3(v3Center.x - v3Ext.x, v3Center.y - v3Ext.y, boundsTr.position.z);
        }

        private void Refresh() {
            if (_scanForWorldLimit) {
                SetBounds(_background.bounds, _background.transform.position, _background.transform);
            }
            Vector3 topRightEdgeScreen = _cam.WorldToScreenPoint(_worldUpperRight);
            Vector3 downLeftEdgeScreen = _cam.WorldToScreenPoint(_worldLowerLeft);
            if (topRightEdgeScreen.x > Screen.width && topRightEdgeScreen.y > Screen.height && downLeftEdgeScreen.x < 0 && downLeftEdgeScreen.y < 0) {
                return;
            }
            var cameraPositionFixPlane = new Plane(Vector3.forward * 10, _cam.transform.position);
            var screenChkPos = new Vector3(MathEx.Max(Screen.width, topRightEdgeScreen.x), MathEx.Max(Screen.height, topRightEdgeScreen.y), topRightEdgeScreen.z);
            Vector3 topRightEdgeScreenFixed = _cam.ScreenToWorldPoint(screenChkPos);
            Vector3 topRightOffsetAtDistance = topRightEdgeScreenFixed - _worldUpperRight;
            Vector3 downLeftEdgeScreenFixed = _cam.ScreenToWorldPoint(new Vector3(MathEx.Min(0, downLeftEdgeScreen.x), MathEx.Min(0, downLeftEdgeScreen.y), downLeftEdgeScreen.z));
            Vector3 downLeftOffsetAtDistance = downLeftEdgeScreenFixed - _worldLowerLeft;
            Vector3 cameraCenterAtDistance = _cam.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, _worldUpperRight.z));
            Vector3 cameraCenterAtDistanceFixed = new Vector3(cameraCenterAtDistance.x - topRightOffsetAtDistance.x - downLeftOffsetAtDistance.x, 
                cameraCenterAtDistance.y - topRightOffsetAtDistance.y - downLeftOffsetAtDistance.y, cameraCenterAtDistance.z);
            Ray rayFromFixedDistanceToCameraPlane = new Ray(cameraCenterAtDistanceFixed, -_cam.transform.forward);
            cameraPositionFixPlane.Raycast(rayFromFixedDistanceToCameraPlane, out var d);
            Vector3 planeHitPoint = rayFromFixedDistanceToCameraPlane.GetPoint(d);
            _cam.transform.position = new Vector3(planeHitPoint.x, planeHitPoint.y, _cam.transform.position.z);
        }

        public void Zoom(float value) {
            if (_cam.orthographic) {
                _cam.orthographicSize = _scrollLimit.Clamp(_cam.orthographicSize + (value * _scrollSpeed));
            }
            else {
                _cam.fieldOfView = _scrollLimit.Clamp(_cam.fieldOfView + (value * _scrollSpeed));
            }
            if (_clampPos) {
                Refresh();
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            UnityEditor.Handles.DrawLine(_worldLowerLeft, _worldUpperRight);
        }
        #endif
    }
}
