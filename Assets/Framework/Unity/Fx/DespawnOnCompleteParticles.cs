using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class DespawnOnCompleteParticles : MonoBehaviour, IPoolEvents, IOnCreate {
        private const float MinimumSpawnTime = 0.1f;

        [SerializeField] private float _checkTime = 0.15f;
        [SerializeField] private bool _unscaled = false;
        [SerializeField] private ParticleSystem _particleSystem = null;

        IEnumerator DespawnParticles() {
            yield return MinimumSpawnTime;
            while (_particleSystem.IsAlive()) {
                if (!gameObject.activeInHierarchy) {
                    yield break;
                }
                yield return _checkTime;
            }
            if (gameObject.activeInHierarchy) {
                ItemPool.Despawn(gameObject);
            }
        }

        public void OnPoolSpawned() {
            TimeManager.StartTask(DespawnParticles(), _unscaled);
        }

        public void OnPoolDespawned() {
        }

        public void OnCreate(PrefabEntity entity) {
            if (_particleSystem == null) {
                _particleSystem = GetComponent<ParticleSystem>();
            }
        }
    }
}