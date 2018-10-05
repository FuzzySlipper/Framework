using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        private Transform _camTr;

        public Camera Cam { get { return _cam; } }

        void Awake() {
            _camTr = _cam.transform;
            if (!_scanForWorldLimit) {
                return;
            }
            var boundsTr = _background.transform;
            var bounds = _background.bounds;
            Vector3 v3Center = Vector3.zero;
            Vector3 v3ext = bounds.extents;

            _worldUpperRight = boundsTr.TransformPoint(new Vector3(v3Center.x + v3ext.x, 0, v3Center.z - v3ext.z));
            _worldLowerLeft = boundsTr.TransformPoint(new Vector3(v3Center.x - v3ext.x, 0, v3Center.z + v3ext.z));

        }

        public void UpdateDrag(Vector3 moveDelta) {
            _camTr.Translate(moveDelta.Multiply(_moveSpeed), UnityEngine.Space.World);
            if (_clampPos) {
                RestrictPosition();
            }
        }

        private void RestrictPosition() {
            //Vector3 minCorner = _riftCam.ViewportToWorldPoint(Vector3.zero);
            //Vector3 maxCorner = _riftCam.ViewportToWorldPoint(Vector3.one);
            //Vector3 boundingBoxSize = (maxCorner - minCorner);
            //float limXLow = _worldLowerLeft.x + boundingBoxSize.x / 2;
            //float limXHigh = _worldUpperRight.x - boundingBoxSize.x / 2;
            //float limYLow = _worldLowerLeft.y + boundingBoxSize.y / 2;
            //float limYHigh = _worldUpperRight.y - boundingBoxSize.y / 2;
            //float x = _worldUpperRight.x / 2 + _worldLowerLeft.x / 2;
            //float y = _worldUpperRight.y / 2 + _worldLowerLeft.y / 2;
            //if (limYLow < limYHigh && limXLow < limXHigh) {
            //    x = Mathf.Clamp(_riftCam.transform.position.x, limXLow, limXHigh);
            //    y = Mathf.Clamp(_riftCam.transform.position.y, limYLow, limYHigh);
            //}

            //_riftCam.transform.position = new Vector3(x, y, _riftCam.transform.position.z);
            Refresh();
        }

        private void Refresh() {
            var topRightEdge = _worldUpperRight;
            var downLeftEdge = _worldLowerLeft;
            Vector3 topRightEdgeScreen = _cam.WorldToScreenPoint(topRightEdge);
            Vector3 downLeftEdgeScreen = _cam.WorldToScreenPoint(downLeftEdge);

            //Debug.Log(downLeftEdgeScreen + "  " + Screen.height);

            // Is the camera out of the map bounds?
            if (topRightEdgeScreen.x < Screen.width || topRightEdgeScreen.y < Screen.height || downLeftEdgeScreen.x > 0 || downLeftEdgeScreen.y > 0) {
                //smack a big plane at the camera position that covers more than the screen is showing
                var cameraPositionFixPlane = new Plane(Vector3.forward * 10, _cam.transform.position);

                var screenChkPos = new Vector3(MathEx.Max(Screen.width, topRightEdgeScreen.x), MathEx.Max(Screen.height, topRightEdgeScreen.y), topRightEdgeScreen.z);
                //move the top right edge back so its inside the screen again
                Vector3 topRightEdgeScreenFixed = _cam.ScreenToWorldPoint(screenChkPos);
                //now we know the offset the camera should move at distance z to fix the top right edge
                Vector3 topRightOffsetAtDistance = topRightEdgeScreenFixed - topRightEdge;

                //this time for the down left edge
                Vector3 downLeftEdgeScreenFixed = _cam.ScreenToWorldPoint(new Vector3(MathEx.Min(0, downLeftEdgeScreen.x), MathEx.Min(0, downLeftEdgeScreen.y), downLeftEdgeScreen.z));
                //now we know the offset the camera should move at distance z to fix the down left edge
                Vector3 downLeftOffsetAtDistance = downLeftEdgeScreenFixed - downLeftEdge;

                //Debug.Log("offset: " + downLeftOffsetAtDistance);


                //where is the center of the screen translated at given distance
                Vector3 cameraCenterAtDistance = _cam.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, topRightEdge.z));
                //now lets offset the center of the screen with the offset we found
                Vector3 cameraCenterAtDistanceFixed = new Vector3(cameraCenterAtDistance.x - topRightOffsetAtDistance.x - downLeftOffsetAtDistance.x, 
                    cameraCenterAtDistance.y - topRightOffsetAtDistance.y - downLeftOffsetAtDistance.y, cameraCenterAtDistance.z);

                //here we generate a ray at the camera center at distance pointing back to the camera
                Ray rayFromFixedDistanceToCameraPlane = new Ray(cameraCenterAtDistanceFixed, -_cam.transform.forward);

                //this is where the magic happens, lets raycast back to the plane i smacked infront of the  camera
                float d;
                cameraPositionFixPlane.Raycast(rayFromFixedDistanceToCameraPlane, out d);

                //where did the raycast hit the camera plane?
                Vector3 planeHitPoint = rayFromFixedDistanceToCameraPlane.GetPoint(d);

                //position camera at the hitpoint we found
                _cam.transform.position = new Vector3(planeHitPoint.x, planeHitPoint.y, _cam.transform.position.z);
            }
        }

        public void Zoom(float value) {
            
            if (_cam.orthographic) {
                _cam.orthographicSize = _scrollLimit.Clamp(_cam.orthographicSize + (value * _scrollSpeed));
            }
            else {
                _cam.fieldOfView = _scrollLimit.Clamp(_cam.fieldOfView + (value * _scrollSpeed));
            }
            RestrictPosition();
        }
    }
}
