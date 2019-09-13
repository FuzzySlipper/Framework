using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    [System.Serializable]
	public sealed class CameraShakeOnDamage : IComponent, IReceive<CollisionEvent> {

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

        public CameraShakeOnDamage(SerializationInfo info, StreamingContext context) {
            _intensityMulti = info.GetValue(nameof(_intensityMulti), _intensityMulti);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_intensityMulti), _intensityMulti);
        }
    }
}
