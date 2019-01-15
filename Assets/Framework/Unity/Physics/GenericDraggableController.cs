using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class GenericDraggableController : MonoSingleton<GenericDraggableController> {

        [SerializeField] private float _spring = 50.0f;
        [SerializeField] private float _damper = 5.0f;
        [SerializeField] private float _drag = 10.0f;
        [SerializeField] private float _angularDrag = 5.0f;
        [SerializeField] private float _springDistance = 0.2f;
        [SerializeField] private float _grabDistance = 15;
        [SerializeField] private Rigidbody _rigidbody = null;
        [SerializeField] private SpringJoint _springJoint = null;
        [SerializeField] private LayerMask _mask = new LayerMask();

        private float _oldDrag;
        private float _oldAngularDrag;
        private bool _dragging = false;
        private Vector3 _screenPoint;
        private float _hitDistance;
        private Rigidbody _target;

        public static bool Dragging { get { return main._dragging; } }
        public static bool HasTarget { get { return main._target != null; } }

        public static void ClearTarget() {
            main._target = null;
        }

        void FixedUpdate() {
            if (!_dragging) {
                return;
            }
            UICenterTarget.SetText("Dragging");
            //      Vector3 point = new Vector3(PlayerInput.LookInput.x, PlayerInput.LookInput.y, _screenPoint.z);
            //Vector3 dest = Player.Camera.ScreenToWorldPoint(point) + _offset;
            //_rigidbody.AddForce((dest - _rigidbody.position) * _pushForce);
            var ray = PlayerInput.GetTargetRay;
            _rigidbody.MovePosition(ray.GetPoint(_hitDistance));
        }



        public bool CanDrag() {
            RaycastHit hit;
            var ray = PlayerInput.GetTargetRay;
            if (!Physics.Raycast(ray, out hit, _grabDistance, _mask)) {
                _target = null;
                return false;
            }
            if (!hit.rigidbody || hit.rigidbody.isKinematic) {
                _target = null;
                return false;
            }
            _springJoint.transform.position = hit.point;
            _target = hit.rigidbody;
            _screenPoint = ray.GetPoint(hit.distance);
            _hitDistance = hit.distance;
            return true;
        }

        public void ToggleDrag() {
            if (_dragging) {
                CancelDrag();
                return;
            }
            if (_target == null) {
                return;
            }
            _rigidbody.isKinematic = true;
            _springJoint.spring = _spring;
            _springJoint.damper = _damper;
            _springJoint.maxDistance = _springDistance;
            _springJoint.anchor = Vector3.zero;
            _springJoint.connectedBody = _target;
            _oldDrag = _springJoint.connectedBody.drag;
            _oldAngularDrag = _springJoint.connectedBody.angularDrag;
            _springJoint.connectedBody.drag = _drag;
            _springJoint.connectedBody.angularDrag = _angularDrag;
            _springJoint.transform.position = _screenPoint;
            _dragging = true;
        }

        public void CancelDrag() {
            _dragging = false;
            UICenterTarget.Clear();
            if (_springJoint == null || _springJoint.connectedBody == null) {
                return;
            }
            _springJoint.connectedBody.drag = _oldDrag;
            _springJoint.connectedBody.angularDrag = _oldAngularDrag;
            _springJoint.connectedBody = null;
        }

    }
}