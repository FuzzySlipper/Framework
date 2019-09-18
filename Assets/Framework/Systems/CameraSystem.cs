using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class CameraSystem : SystemBase, IReceive<CollisionEvent> {

        public CameraSystem() {
            EntityController.RegisterReceiver<CameraShakeOnDamage>(this);
        }

        public void Handle(CollisionEvent arg) {
            var cameraShakeOnDamage = arg.Target.Get<CameraShakeOnDamage>();
            if (cameraShakeOnDamage == null) {
                return;
            }
            FirstPersonCamera.AddForce(-arg.HitNormal * cameraShakeOnDamage.IntensityMulti, true);
        }
    }
}