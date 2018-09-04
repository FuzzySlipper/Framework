using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [RequireComponent(typeof(Rigidbody))] public class RigidbodyFreeze : MonoBehaviour {


        private Rigidbody _rigidbody;

        private bool _frozen = false;
        private RigidbodySettings _rigidbodySetup = new RigidbodySettings();

        void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbodySetup.Setup(_rigidbody);
        }

        void OnEnable() {
            MessageKit.addObserver(Messages.PauseChanged, CheckPause);
        }

        void OnDisable() {
            MessageKit.removeObserver(Messages.PauseChanged, CheckPause);
        }

        private void CheckPause() {
            if (_frozen == Game.Paused) {
                return;
            }
            _frozen = Game.Paused;
            if (_frozen) {
                _rigidbodySetup.Freeze();
            }
            else {
                _rigidbodySetup.Restore();
            }
        }
    }

    [System.Serializable] public class RigidbodySettings {

        private static RigidbodyConstraints _freezeConstraints = RigidbodyConstraints.FreezeAll;

        private Rigidbody _rigidbody;
        private bool _useGravity;
        private bool _isKinematic;
        private bool _detectCollisions;
        private bool _freezeRotation;
        private RigidbodyConstraints _constraints;
        private Vector3 _velocity;

        public bool IsFrozen { get; private set; }

        public RigidbodySettings() {
        }

        public RigidbodySettings(Rigidbody rigidbody) {
            Setup(rigidbody);
        }

        public void Setup(Rigidbody rigidbody) {
            IsFrozen = false;
            _rigidbody = rigidbody;
            Set();
        }

        public void Set() {
            _velocity = _rigidbody.velocity;
            _useGravity = _rigidbody.useGravity;
            _isKinematic = _rigidbody.isKinematic;
            _detectCollisions = _rigidbody.detectCollisions;
            _freezeRotation = _rigidbody.freezeRotation;
            _constraints = _rigidbody.constraints;
        }

        public void Restore() {
            _rigidbody.useGravity = _useGravity;
            _rigidbody.isKinematic = _isKinematic;
            _rigidbody.detectCollisions = _detectCollisions;
            _rigidbody.freezeRotation = _freezeRotation;
            _rigidbody.constraints = _constraints;
            _rigidbody.velocity = _velocity;
            IsFrozen = false;
        }

        public void Freeze() {
            Set();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _rigidbody.detectCollisions = false;
            _rigidbody.freezeRotation = true;
            _rigidbody.constraints = _freezeConstraints;
            _rigidbody.velocity = Vector3.zero;
            IsFrozen = true;
        }

        public void OverrideVelocity(Vector3 force) {
            _velocity = force;
        }
    }

    public class TransformSaver {

        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;
        private Transform _tr;

        public TransformSaver(Transform tr) {
            _tr = tr;
            _position = tr.localPosition;
            _rotation = tr.localRotation;
            _scale = tr.localScale;
        }

        public void Restore() {
            _tr.localScale = _scale;
            _tr.localPosition = _position;
            _tr.localRotation = _rotation;
        }
    }
}