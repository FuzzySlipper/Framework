using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityModifierSystem : SystemBase, IMainSystemUpdate {

        public void OnSystemUpdate(float dt) {
            var modContainers = EntityController.GetComponentArray<ModifiersContainer>();
            if (modContainers != null) {
                modContainers.RunAction(m => m.Update());
            }
            MessageKit.post(Messages.ModifiersUpdated);
        }
    }
}
