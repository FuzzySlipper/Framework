using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DespawnTimer : IComponent {
        private int _owner = -1;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (_owner < 0 || !_autoStart) {
                    return;
                }
                StartTimer();
            }
        }
        public float Time { get; set; }
        public bool Unscaled { get; }
        public IEntityPool Pool;

        private bool _autoStart;

        public DespawnTimer(float time, bool unscaled, IEntityPool pool, bool autoStart = true) {
            Time = time;
            Unscaled = unscaled;
            Pool = pool;
            _autoStart = autoStart;
        }

        public void StartTimer() {
            TimeManager.StartTask(WaitTimer(), Unscaled);
        }

        private IEnumerator WaitTimer() {
            var entity = this.GetEntity();
            if (entity == null) {
                yield break;
            }
            while (!entity.IsDestroyed()) {
                yield return Time;
                entity.Destroy(Pool);
            }
        }
    }
}
