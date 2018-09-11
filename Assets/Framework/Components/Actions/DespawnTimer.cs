using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DespawnTimer : IComponent {
        private int _owner;
        public int Owner {
            get { return _owner; }
            set {
                if (_owner == value) {
                    return;
                }
                _owner = value;
                if (_owner < 0) {
                    return;
                }
                TimeManager.Start(WaitTimer(), Unscaled);
            }
        }
        public float Time { get; }
        public bool Unscaled { get; }

        public DespawnTimer(float time, bool unscaled) {
            Time = time;
            Unscaled = unscaled;
        }

        private IEnumerator WaitTimer() {
            yield return Time;
            var entity = this.GetEntity();
            if (entity != null && !entity.IsDestroyed()) {
                entity.Destroy();
            }
        }


    }
}
