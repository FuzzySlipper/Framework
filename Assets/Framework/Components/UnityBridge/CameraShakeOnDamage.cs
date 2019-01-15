using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class CameraShakeOnDamage : IComponent, IReceive<DamageEvent> {

        public int Owner { get; set; }
        private float _intensity;

        public CameraShakeOnDamage(int owner, float intensity = 150f) {
            Owner = owner;
            _intensity = intensity;
        }

        public void Handle(DamageEvent arg) {
            if (arg.Amount > 0.5f) {
                this.GetEntity().Post(new CameraShakeEvent(_intensity));
            }
        }
    }
}
