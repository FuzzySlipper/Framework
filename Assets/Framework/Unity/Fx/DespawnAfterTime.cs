using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class DespawnAfterTime : MonoBehaviour {

        [SerializeField] private float _time = 0.15f;
        [SerializeField] private bool _unscaled = false;

        void OnEnable() {
            if (_unscaled) {
                TimeManager.StartUnscaled(Despawn());
            }
            else {
                TimeManager.StartTask(Despawn());
            }
        }

        IEnumerator Despawn() {
            yield return _time;
            ItemPool.Despawn(gameObject);
        }
    }
}