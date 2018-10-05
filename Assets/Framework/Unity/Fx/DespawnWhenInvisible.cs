using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class DespawnWhenInvisible : MonoBehaviour, IPoolEvents {

        [SerializeField] private float _timeToDespawn = 5f;
        [SerializeField] private bool _despawnParent = false;

        private bool _visible = true;


        void OnBecameInvisible() {
            _visible = false;
            TimeManager.StartTask(CheckVisibleStatus());
        }

        void OnBecameVisible() {
            _visible = true;
        }

        private IEnumerator CheckVisibleStatus() {
            yield return _timeToDespawn;
            if (!_visible) {
                if (_despawnParent) {
                    ItemPool.Despawn(transform.parent.gameObject);
                }
                else {
                    ItemPool.Despawn(gameObject);
                }
            }
        }

        public void OnPoolSpawned() {
            _visible = true;
        }

        public void OnPoolDespawned() {
        }
    }
}