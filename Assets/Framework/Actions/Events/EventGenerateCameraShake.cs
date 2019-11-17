using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GenerateGenerateCameraShake : IActionEventHandler {
        public Vector3 Shake { get; }

        public GenerateGenerateCameraShake(Vector3 percentShake) {
            Shake = percentShake;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            World.Get<EntityEventSystem>().Post(new CameraPositionForceEvent(Shake));
        }
    }
}
