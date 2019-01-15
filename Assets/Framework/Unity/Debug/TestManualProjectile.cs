using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class TestManualProjectile : MonoBehaviour {

        [SerializeField] private float _angle = 15;
        [SerializeField] private float _speed = 9.8f;
        [SerializeField] private float _lookAhead = 0.25f;
        [SerializeField] private Vector3 _boundsCheckSize = new Vector3(0.1f,0.1f,0.01f);
        [SerializeField] private Transform _target = null;

        public Vector3 LastTestStart;

        private RaycastHit[] _rayHits = new RaycastHit[5];
        private Vector3 _moveVector;
        private float _elapsedTime;
        private float _duration;
        private Vector3 _targetPos;
        private Vector3 _lastPos;

        public void Test(bool testCollision) {
            if (_target == null) {
                return;
            }
            LastTestStart = transform.position;
            _targetPos = _target.transform.position;
            _lastPos = transform.position;
            CalculateFlight(_targetPos);
            TimeManager.StartUnscaled(TurnUpdate(testCollision));
        }

        private IEnumerator TurnUpdate(bool testCollision) {
            while (_elapsedTime < _duration) {
                _elapsedTime += TimeManager.DeltaUnscaled;
                transform.Translate(0, (_moveVector.y - (_speed * _elapsedTime)) * TimeManager.DeltaTime, _moveVector.z * TimeManager.DeltaTime);
                if (testCollision) {
                    CheckCollision();
                }
                yield return null;
            }
            Debug.LogFormat("reached destination {0} in {1}", _targetPos, _elapsedTime);
        }

        private void CalculateFlight(Vector3 target) {
            float targetDistance = Vector3.Distance(transform.position, target);
            // Calculate the velocity needed to throw the object to the target at specified angle.
            float projectileVelocity = targetDistance / (Mathf.Sin(2 * _angle * Mathf.Deg2Rad) / _speed);
            _moveVector.z = Mathf.Sqrt(projectileVelocity) * Mathf.Cos(_angle * Mathf.Deg2Rad);
            _moveVector.y = Mathf.Sqrt(projectileVelocity) * Mathf.Sin(_angle * Mathf.Deg2Rad);
            // Calculate flight time.
            _duration = targetDistance / _moveVector.z;
            // Rotate projectile to face the target.
            transform.rotation = Quaternion.LookRotation(target - transform.position);
            _elapsedTime = 0;
        }

        private bool CheckCollision() {
            int hitLimit;
            if (_lastPos != transform.position) {
                var dir = transform.position - _lastPos;
                hitLimit = Physics.BoxCastNonAlloc(_lastPos, _boundsCheckSize, dir.normalized, _rayHits, transform.rotation, dir.magnitude, LayerMasks.DefaultCollision);
                if (CheckListForCollision(hitLimit)) {
                    return true;
                }
            }
            _lastPos = transform.position;
            hitLimit = Physics.RaycastNonAlloc(transform.position, transform.forward, _rayHits, _lookAhead, LayerMasks.DefaultCollision);
            if (CheckListForCollision(hitLimit)) {
                return true;
            }
            return false;
        }

        private bool CheckListForCollision(int hitLimit) {
            for (int i = 0; i < hitLimit; i++) {
                var collision = _rayHits[i].collider;
                if (collision.transform == transform) {
                    continue;
                }
                Debug.DrawLine(transform.position, _rayHits[i].point, Color.magenta, 4f);
                var pnt = _rayHits[i].point;
                var dir = -_rayHits[i].normal;
                Debug.LogFormat("hit {0} at {1} dir {2}", collision.name, pnt, dir);
                //return true;
            }
            return false;
        }
    }
}
