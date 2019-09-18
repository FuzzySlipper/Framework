using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class DespawnTimer : IComponent {
        public float Time { get; set; }
        public bool Unscaled { get; }

        public DespawnTimer(float time, bool unscaled, bool autoStart = true) {
            Time = time;
            Unscaled = unscaled;
            if (autoStart) {
                StartTimer();
            }
        }

        public void StartTimer() {
            TimeManager.StartTask(WaitTimer(), Unscaled);
        }

        private IEnumerator WaitTimer() {
            var entity = this.GetEntity();
            if (entity == null) {
                yield break;
            }
            var finishTime = (Unscaled ? TimeManager.TimeUnscaled : TimeManager.Time) + Time;
            while (true) {
                if (entity.IsDestroyed()) {
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

        public DespawnTimer(SerializationInfo info, StreamingContext context) {
            Time = info.GetValue(nameof(Time), Time);
            Unscaled = info.GetValue(nameof(Unscaled), Unscaled);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Time), Time);
            info.AddValue(nameof(Unscaled), Unscaled);
        }
    }
}
