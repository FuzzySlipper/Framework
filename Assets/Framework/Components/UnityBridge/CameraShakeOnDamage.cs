using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class CameraShakeOnDamage : IComponent {

        public float IntensityMulti { get; }

        public CameraShakeOnDamage(float intensityMulti = 2f) {
            IntensityMulti = intensityMulti;
        }

        //public void Handle(DamageEvent arg) {
        //    if (arg.Amount > 0.5f) {
        //        FirstPersonCamera.AddForce(arg.);
        //        this.GetEntity().Post(new CameraShakeEvent(_intensityMulti));
        //    }
        //}
        
        public CameraShakeOnDamage(SerializationInfo info, StreamingContext context) {
            IntensityMulti = info.GetValue(nameof(IntensityMulti), IntensityMulti);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(IntensityMulti), IntensityMulti);
        }
    }
}
