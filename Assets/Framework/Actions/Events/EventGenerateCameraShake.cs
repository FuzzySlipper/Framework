using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GenerateGenerateCameraShake : IActionEvent {
        public Vector3 Shake { get; }

        public GenerateGenerateCameraShake(Vector3 percentShake) {
            Shake = percentShake;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            FirstPersonCamera.AddForce(Shake, false);
        }
    }
}
