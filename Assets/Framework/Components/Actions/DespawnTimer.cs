using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class DespawnTimer : IComponent, IReceive<EntityDestroyed>{
        public float Time { get; set; }
        public bool Unscaled { get; }

        private bool _canDespawn;

        public DespawnTimer(float time, bool unscaled, bool autoStart = true) {
            Time = time;
            Unscaled = unscaled;
            if (autoStart) {
                StartTimer();
            }
        }

        public void StartTimer() {
            _canDespawn = true;
            TimeManager.StartTask(WaitTimer(), Unscaled);
        }

        private IEnumerator WaitTimer() {
            var entity = this.GetEntity();
            if (entity == null) {
                yield break;
            }
            var finishTime = (Unscaled ? TimeManager.TimeUnscaled : TimeManager.Time) + Time;
            while (true) {
                if (!_canDespawn || entity.IsDestroyed()) {
                    break;
                }
                var compareTime = Unscaled ? TimeManager.TimeUnscaled : TimeManager.Time;
                if (compareTime >= finishTime) {
                    entity.Destroy();
                    break;
                }
                yield return null;
            }
        }

        public void Handle(EntityDestroyed arg) {
            _canDespawn = false;
        }

        public DespawnTimer(SerializationInfo info, StreamingContext context) {
            Time = info.GetValue(nameof(Time), Time);
            Unscaled = info.GetValue(nameof(Unscaled), Unscaled);
            _canDespawn = info.GetValue(nameof(_canDespawn), _canDespawn);
            if (_canDespawn) {
                StartTimer();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Time), Time);
            info.AddValue(nameof(Unscaled), Unscaled);
            info.AddValue(nameof(_canDespawn), _canDespawn);
        }
    }
}
