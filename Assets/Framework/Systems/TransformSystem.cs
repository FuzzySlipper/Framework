using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    [AutoRegister]
    public sealed class TransformSystem : SystemBase, IReceive<DeathEvent> {

        public TransformSystem() {
            EntityController.RegisterReceiver<DisableTrOnDeath>(this);
        }
        
        public void Handle(DeathEvent arg) {
            if (arg.Target.Entity.HasComponent<DisableTrOnDeath>()) {
                arg.Target.Tr.gameObject.SetActive(false);
            }
        }
    }
}
