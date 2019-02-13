using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class QuadFlash : MonoBehaviour, IPoolEvents {

        [SerializeField] private float _spawnTime = 0.075f;

        public void OnPoolSpawned() {
            transform.rotation *= Quaternion.Euler(0, 0, Random.Range(-360, 360));
            TimeManager.StartTask(WaitForDespawn());
        }

        private IEnumerator WaitForDespawn() {
            yield return _spawnTime;
            ItemPool.Despawn(gameObject);
        }

        public void OnPoolDespawned() {

        }
    }
}