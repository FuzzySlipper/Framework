using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class SpawnSystem : SystemBase, IMainSystemUpdate, IReceive<ProjectileSpawned>, IReceive<ProjectileDespawned> {

        public SpawnSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(DespawnTimer)
            }));
        }

        private ComponentArray<DespawnTimer> _despawnArray;

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_despawnArray == null) {
                _despawnArray = EntityController.GetComponentArray<DespawnTimer>();
            }
            if (_despawnArray != null) {
                foreach (DespawnTimer despawnTimer in _despawnArray) {
                    if (despawnTimer.FinishItem < 0) {
                        continue;
                    }
                    var compareTime = despawnTimer.Unscaled ? TimeManager.TimeUnscaled : TimeManager.Time;
                    if (despawnTimer.FinishItem <= compareTime) {
                        despawnTimer.GetEntity().Destroy();
                    }
                }
            }
        }

        public void Handle(ProjectileSpawned arg) {
            var despawnTimer = arg.Entity.Get<DespawnTimer>();
            if (despawnTimer != null) {
                despawnTimer.FinishItem = despawnTimer.Unscaled ? TimeManager.TimeUnscaled : TimeManager.Time + despawnTimer.Length;
            }
        }

        public void Handle(ProjectileDespawned arg) {
            var despawnTimer = arg.Entity.Get<DespawnTimer>();
            if (despawnTimer != null) {
                despawnTimer.FinishItem = -1;
            }
        }
    }
}
