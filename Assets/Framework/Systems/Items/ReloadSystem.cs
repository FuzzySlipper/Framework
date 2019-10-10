using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class ReloadSystem : SystemBase, IReceive<AnimationEventTriggered> {

        public ReloadSystem() {
            World.Get<AnimationEventSystem>().Register(AnimationEvents.Reload, this);
        }

        public void Handle(AnimationEventTriggered arg) {
            var reloadComponent = arg.Entity.Get<ReloadWeaponComponent>();
            if (reloadComponent?.Ammo != null) {
                reloadComponent.Ammo.TryLoadOneAmmo(arg.Entity);
            }
        }
    }
}
