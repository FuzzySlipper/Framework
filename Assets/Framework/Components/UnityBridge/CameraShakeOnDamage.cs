using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class CameraShakeOnDamage : IComponent, IReceive<CollisionEvent> {

        public int Owner { get; set; }
        private float _intensityMulti;

        public CameraShakeOnDamage(float intensityMulti = 2f) {
            _intensityMulti = intensityMulti;
        }

        //public void Handle(DamageEvent arg) {
        //    if (arg.Amount > 0.5f) {
        //        FirstPersonCamera.AddForce(arg.);
        //        this.GetEntity().Post(new CameraShakeEvent(_intensityMulti));
        //    }
        //}
        public void Handle(CollisionEvent arg) {
            FirstPersonCamera.AddForce(-arg.HitNormal * _intensityMulti, true);
        }
    }
}
