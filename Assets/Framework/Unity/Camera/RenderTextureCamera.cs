using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class RenderTextureCamera : MonoSingleton<RenderTextureCamera> {

        [SerializeField] private Camera _camera;
        [SerializeField] private float _viewOffset = 14f;
        [SerializeField] private float _maxBottomError = 0.01f;
        [SerializeField] private float _moveOffset = 1;
        [SerializeField] private float _positionMarginMultiplier = 1.5f;
        [SerializeField] private bool _debug = false;
        [SerializeField] private Transform _target = null;

        private float _minX, _minY, _maxX, _maxY;
        private WhileLoopLimiter _loopLimiter = new WhileLoopLimiter(150);

        private static Camera Camera { get { return main._camera; } }

        private static FieldInfo _canvasHackField;
        private static object _canvasHackObject;

        void Awake() {
            _canvasHackField = typeof(Canvas).GetField("willRenderCanvases", BindingFlags.NonPublic | BindingFlags.Static);
            _canvasHackObject = _canvasHackField.GetValue(null);
        }

        public static void TakePicture(Transform parent, Bounds bounds, DirectionsEight dir, RenderTexture texture) {
            Camera.enabled = true;
            Camera.transform.parent = parent;
            Camera.targetTexture = texture;
            SpriteFacingControl.SetCameraPos(Camera, dir, main._viewOffset, 0f);
            if (main._debug) {
                DebugExtension.DebugArrow(Camera.transform.position, Camera.transform.forward, Color.blue, 5f);
            }
            main.Zoom(bounds);
            main.PositionBottom(parent.position);
            _canvasHackField.SetValue(null, null);
            Camera.Render();
           _canvasHackField.SetValue(null, _canvasHackObject);
            Camera.Render();
            Camera.enabled = false;
            //Camera.targetTexture = null;
            Camera.transform.parent = null;
        }

        [Button("PositionCamera")]
        void PositionCamera() {
            if (_camera == null) {
                _camera = GetComponent<Camera>();
            }
            Game.SpriteCamera = _camera;
            var objectToView = _target;
            if (objectToView == null) {
                return;
            }
            var objs = objectToView.GetComponentsInChildren<Renderer>();
            var bounds = objs[0].bounds;
            for (int i = 1; i < objs.Length; i++) {
                bounds.Encapsulate(objs[i].bounds);
            }
            Vector3 objectFrontCenter = bounds.center - objectToView.transform.forward * bounds.extents.z;
            //Get the far side of the triangle by going up from the center, at a 90 degree angle of the camera's forward vector.
            Vector3 triangleFarSideUpAxis = Quaternion.AngleAxis(90, objectToView.transform.right) * transform.forward;
            //Calculate the up point of the triangle.
            Vector3 triangleUpPoint = objectFrontCenter + triangleFarSideUpAxis * bounds.extents.y * _positionMarginMultiplier;
            //The angle between the camera and the top point of the triangle is half the field of view.
            //The tangent of this angle equals the length of the opposing triangle side over the desired distance between the camera and the object's front.
            float desiredDistance = Vector3.Distance(triangleUpPoint, objectFrontCenter) / Mathf.Tan(Mathf.Deg2Rad * GetComponent<Camera>().fieldOfView / 2);
            transform.position = -transform.forward * desiredDistance + objectFrontCenter;
            Zoom(bounds);
            PositionBottom(objectToView.transform.position);
        }

        [Button("Zoom")]
        private void Zoom() {
            if (_camera == null) {
                _camera = GetComponent<Camera>();
            }
            Game.SpriteCamera = _camera;
            var objectToView = _target;
            if (objectToView == null) {
                return;
            }
            var objs = objectToView.GetComponentsInChildren<Renderer>();
            var bounds = objs[0].bounds;
            for (int i = 1; i < objs.Length; i++) {
                bounds.Encapsulate(objs[i].bounds);
            }
            var dir = SpriteFacingControl.GetCameraSide(SpriteFacing.Eightway, objectToView, objectToView, 0f, out var inMargin);
            SpriteFacingControl.SetCameraPos(Camera, dir, _viewOffset, 0f);
            if (main._debug) {
                DebugExtension.DebugBounds(bounds, Color.red, 5f);
            }
            Zoom(bounds);
            PositionBottom(objectToView.transform.position);
        }

        private void PositionBottom(Vector3 point) {
            _loopLimiter.Reset();
            while (_loopLimiter.Advance()) {
                var screenPnt = _camera.WorldToScreenPoint(point).y;
                _camera.transform.Translate(0, screenPnt > 0 ? _moveOffset : -_moveOffset, 0);
                if (Math.Abs(screenPnt) < _maxBottomError) {
                    if (_debug) {
                        Debug.LogFormat("Hit screen point at {0} after {1}", _camera.WorldToViewportPoint(point), _loopLimiter.Count);
                    }
                    break;
                }
            }
        }
        
        private void Zoom(Bounds bounds) {
            _minX = _minY = Mathf.Infinity;
            _maxX = _maxY = Mathf.NegativeInfinity;
            Vector3 point = bounds.center + bounds.extents;
            ClampBoundingMinMax(point);
            point.x -= bounds.size.x;
            ClampBoundingMinMax(point);
            point.y -= bounds.size.y;
            ClampBoundingMinMax(point);
            point.x += bounds.size.x;
            ClampBoundingMinMax(point);
            point.z -= bounds.size.z;
            ClampBoundingMinMax(point);
            point.x -= bounds.size.x;
            ClampBoundingMinMax(point);
            point.y += bounds.size.y;
            ClampBoundingMinMax(point);
            point.x += bounds.size.x;
            ClampBoundingMinMax(point);
            _camera.orthographicSize = (1f) * Mathf.Max(_maxY - _minY, (_maxX - _minX) / _camera.aspect) * 0.5f;
            _camera.transform.position = new Vector3(_camera.transform.position.x, bounds.center.y, _camera.transform.position.z);
            
        }

        private void ClampBoundingMinMax(Vector3 point) {
            Vector3 localPoint = _camera.transform.InverseTransformPoint(point);
            if (localPoint.x < _minX)
                _minX = localPoint.x;
            if (localPoint.x > _maxX)
                _maxX = localPoint.x;
            if (localPoint.y < _minY)
                _minY = localPoint.y;
            if (localPoint.y > _maxY)
                _maxY = localPoint.y;
        }
    }
}
