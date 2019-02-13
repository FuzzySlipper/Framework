using UnityEngine;
using System.Collections;
using Sirenix.Utilities;

namespace PixelComrades {
    public class DespawnOnCompleteParticles : MonoBehaviour, IPoolEvents, IOnCreate, ISystemUpdate {
        private const float MinimumSpawnTime = 0.1f;

        [SerializeField] private float _checkTime = 0.15f;
        [SerializeField] private float _timeout = 5f;
        [SerializeField] private bool _unscaled = false;
        [SerializeField] private ParticleSystem _particleSystem = null;

        private float _start;

        public bool Unscaled { get { return _unscaled; } }
        private float Time { get { return _unscaled ? TimeManager.TimeUnscaled : TimeManager.Time ;}}

        IEnumerator DespawnParticles() {
            var start =  Time;
            yield return MinimumSpawnTime;
            while (!_particleSystem.SafeIsUnityNull() && _particleSystem.IsAlive()) {
                if (start + _timeout > Time) {
                    yield break;
                }
                if (!gameObject.activeInHierarchy) {
                    yield break;
                }
                yield return _checkTime;
            }
            if (!gameObject.SafeIsUnityNull() && gameObject.activeInHierarchy) {
                ItemPool.Despawn(gameObject);
            }
        }

        public void OnPoolSpawned() {
            //TimeManager.StartTask(DespawnParticles(), _unscaled);
            _start = Time;
        }

        public void OnPoolDespawned() {
        }

        public void OnCreate(PrefabEntity entity) {
            if (_particleSystem == null) {
                _particleSystem = GetComponent<ParticleSystem>();
            }
        }
        
        public void OnSystemUpdate(float dt) {
            if (_start + MinimumSpawnTime < Time) {
                return;
            }
            if (_start + _timeout < Time) {
                ItemPool.Despawn(gameObject);
                return;
            }
            if (_particleSystem.SafeIsUnityNull()) {
                return;
            }
            if (!_particleSystem.IsAlive()) {
                ItemPool.Despawn(gameObject);
            }
        }
    }
}