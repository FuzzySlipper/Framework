using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class PrefabEntityRaycastPlace : PrefabEntity {
        [SerializeField] private LayerMask _rayCastMask = new LayerMask();
        [SerializeField] private float _distance = 100;
        [SerializeField] private float _delay = 0;
        [SerializeField] private bool _unscaled = true;
        [SerializeField] private bool _colliderAdjust = true;
        [SerializeField] private Directions _direction = Directions.Down;

        private Rigidbody _rigidbody;
        private Collider _collider;
        private bool _positioned = false;

        void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            if (_colliderAdjust) {
                _collider = GetComponent<Collider>();
            }
        }

        public override void Register(bool isSceneObject, bool isCulled) {
            base.Register(isSceneObject, isCulled);
            _positioned = false;
        }

        public override void SetActive(bool status) {
            base.SetActive(status);
            if (!_positioned && status) {
                _positioned = true;
                if (_delay > 0) {
                    TimeManager.PauseFor(_delay, _unscaled, SetPosition);
                }
                else {
                    SetPosition();
                }
            }
        }

        private void SetPosition() {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, _direction.ToV3());
            if (Physics.Raycast(ray, out hit, _distance, _rayCastMask)) {
                var hitPosition = hit.point;
                if (_colliderAdjust && _collider != null) {
                    hitPosition += (transform.position - hit.point).normalized * _collider.bounds.extents.magnitude;
                }
                if (_rigidbody != null) {
                    _rigidbody.MovePosition(hitPosition);
                }
                else {
                    transform.position = hitPosition;
                }
            }
        }
    }
}