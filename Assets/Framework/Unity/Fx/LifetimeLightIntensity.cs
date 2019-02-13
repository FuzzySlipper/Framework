//using ParticlePlayground;
using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class LifetimeLightIntensity : MonoBehaviour, IPoolEvents, IOnCreate {

        [SerializeField] private AnimationCurve _lightIntensity = AnimationCurve.Linear(0, 1, 0.5f, 0);
        //public PlaygroundParticlesC ParticleC;
        [SerializeField] private float _defaultDuration = 0.5f;
        [SerializeField] private bool _unscaled = false;
        [SerializeField] private bool _despawnOnComplete = false;

        private Light _light;
        private float _maxIntensity;
        private LerpHolder _lerp = new LerpHolder();
        private ParticleSystem _particle;

        public void OnCreate(PrefabEntity entity) {
            _particle = GetComponent<ParticleSystem>();
            _light = GetComponent<Light>();
            _maxIntensity = _light.intensity;
        }

        public void OnPoolSpawned() {
            float duration;
            //if (!ParticleC== null) {
            //    duration = ParticleC.lifetime + ParticleC.lifetimeOffset;
            //}
            if (_particle != null) {
                duration = _particle.main.duration;
            }
            else {
                duration = _defaultDuration;
            }
            _lerp.RestartLerp(0, 1, duration);
            if (_unscaled) {
                TimeManager.StartUnscaled(UpdateLight());
            }
            else {
                TimeManager.StartTask(UpdateLight());
            }
        }

        public void OnPoolDespawned() {
        }

        private IEnumerator UpdateLight() {
            _light.enabled = true;
            while (!_lerp.IsFinished) {
                _light.intensity = _maxIntensity * _lightIntensity.Evaluate(_lerp.GetLerpValue());
                yield return null;
            }
            if (_despawnOnComplete) {
                ItemPool.Despawn(gameObject);
            }
            else {
                _light.enabled = false;
            }
        }

    }
}