using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SimpleProjectileSpawner : IComponent {

        public CachedTransform ShootPivot = new CachedTransform();
        public string ProjectileId;
        public FloatRange ShootCooldown;
        public Timer ShootTimer = new Timer();

        public SimpleProjectileSpawner(Transform shootPivot, string projectileId, FloatRange shootCooldown) {
            ShootPivot.Set(shootPivot);
            ProjectileId = projectileId;
            ShootCooldown = shootCooldown;
        }

        public void Fire() {
            Fire(ShootPivot.Tr.position + ShootPivot.Tr.forward * 1000);
        }

        public void Fire(Vector3 targetPos) {
            var position = ShootPivot.Tr.position;
            World.Get<ProjectileSystem>().SpawnProjectile(this.GetEntity(), ProjectileId, targetPos, position, Quaternion
            .LookRotation(targetPos - position));
            ShootTimer.StartNewTime(ShootCooldown.Get());
        }
        
        public SimpleProjectileSpawner(SerializationInfo info, StreamingContext context) {
            ProjectileId = info.GetValue(nameof(ProjectileId), ProjectileId);
            ShootCooldown = info.GetValue(nameof(ShootCooldown), ShootCooldown);
            ShootTimer = info.GetValue(nameof(ShootTimer), ShootTimer);
            ShootPivot = info.GetValue(nameof(ShootPivot), ShootPivot);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(ProjectileId), ProjectileId);
            info.AddValue(nameof(ShootCooldown), ShootCooldown);
            info.AddValue(nameof(ShootTimer), ShootTimer);
            info.AddValue(nameof(ShootPivot), ShootPivot);
        }
    }
}
