using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [ExecuteInEditMode]
    public class CameraParallax : MonoBehaviour {

        [SerializeField] private Camera _farCamera;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _nearCamera;

        private void LateUpdate() {
            UpdateCameras();
        }

        private void OnEnable() {
            InitCameras();
        }

        public void InitCameras() {
            if (_mainCamera == null) {
                return;
            }
            if (_farCamera != null) {
                _farCamera.transform.localPosition = Vector3.zero;
                _farCamera.transform.rotation = Quaternion.identity;
                _farCamera.transform.localScale = Vector3.one;
                _farCamera.orthographic = false;
                _farCamera.clearFlags = CameraClearFlags.SolidColor;
                _farCamera.transparencySortMode = TransparencySortMode.Orthographic;
            }
            _mainCamera.orthographic = true;
            _mainCamera.clearFlags = CameraClearFlags.Nothing;
            _mainCamera.depth = -1;
            if (_nearCamera != null) {
                _nearCamera.transform.localPosition = Vector3.zero;
                _nearCamera.transform.rotation = Quaternion.identity;
                _nearCamera.transform.localScale = Vector3.one;
                _nearCamera.orthographic = false;
                _nearCamera.clearFlags = CameraClearFlags.Depth;
                _nearCamera.transparencySortMode = TransparencySortMode.Orthographic;
            }
        }

        public void UpdateCameras() {
            if (_mainCamera == null || _farCamera == null || _nearCamera == null)
                return;

            // orthoSize
            var a = _mainCamera.orthographicSize;
            // distanceFromOrigin
            //var b = Mathf.Abs(_mainCamera.transform.position.z);

            ////change clipping planes based on main camera z-position
            //_farCamera.nearClipPlane = b;
            //_farCamera.farClipPlane = _mainCamera.farClipPlane;
            //_nearCamera.farClipPlane = b;
            //_nearCamera.nearClipPlane = _mainCamera.nearClipPlane;

            //update field fo view for parallax cameras a/b
            var fieldOfView = Mathf.Atan(a) * Mathf.Rad2Deg * 2f;
            _nearCamera.fieldOfView = _farCamera.fieldOfView = fieldOfView;
        }
    }
}
