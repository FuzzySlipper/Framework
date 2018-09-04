using UnityEngine;
using System.Collections;

namespace PixelComrades {
    [RequireComponent(typeof(LineRenderer))] public class LaserController : MonoBehaviour, ISystemUpdate, IPoolEvents, IOnCreate {

        private const float MaxBeam = 100f;

        [SerializeField] private bool _animateUv = true;
        [SerializeField] private Texture[] _beamFrames = new Texture[0];
        [SerializeField] private float _animationFrameStep = 0.001f;
        [SerializeField] private float _uvAnimationTime = -6;
        [SerializeField] private float _defaultBeamScale = 1;
        [SerializeField] private GameObject _impact = null;

        private float _beamLength;
        private int _frameNo;
        private float _initialBeamOffset;
        private LineRenderer _lineRenderer;

        public void OnCreate(PrefabEntity entity) {
            _lineRenderer = GetComponent<LineRenderer>();
            if (!_animateUv && _beamFrames.Length > 0) {
                _lineRenderer.material.mainTexture = _beamFrames[0];
            }
            _initialBeamOffset = Random.Range(0f, 5f);
        }

        public void OnPoolSpawned() {
            _impact.SetActive(false);
            _frameNo = 0;
            Animate();
        }

        public void OnPoolDespawned() {
        }

        public void SetBeamHit(Vector3 hitPoint, Vector3 hitNormal) {
            _beamLength = Vector3.Distance(transform.position, hitPoint);
            _impact.SetActive(true);
            _impact.transform.position = hitPoint + hitNormal * 0.2f;
            SetBeam();
        }

        public void SetBeamLength(float length) {
            _beamLength = length;
            _impact.SetActive(false);
            SetBeam();
        }

        private void SetBeam() {
            var propMult = MaxBeam * (_defaultBeamScale / 10f);
            _lineRenderer.material.SetTextureScale("_MainTex", new Vector2(propMult, 1f));
            _lineRenderer.SetPosition(1, new Vector3(0f, 0f, _beamLength));
        }

        private void Animate() {
            if (_beamFrames.Length <= 1) {
                return;
            }
            _lineRenderer.material.mainTexture = _beamFrames[_frameNo];
            _frameNo++;
            if (_frameNo == _beamFrames.Length) {
                _frameNo = 0;
            }
        }

        public bool Equals(ISystemUpdate other) {
            var script = other as LaserController;
            if (script != null) {
                return script == this;
            }
            return false;
        }

        public float Freq { get { return _beamFrames.Length <= 1 ? 0 : _animationFrameStep; } }
        public bool Unscaled { get { return false; } }

        public void OnSystemUpdate(float delta) {
            Animate();
            if (!_animateUv) {
                return;
            }
            _lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(TimeManager.Time * _uvAnimationTime + _initialBeamOffset, 0f));
        }

    }
}