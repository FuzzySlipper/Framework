using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CostItself : CommandCost {

        public override void ProcessCost(Entity entity) {
            entity.Get<EntityContainer>(f => ContainerSystem.TryRemoveFromContainer(f, entity));
            entity.Destroy();
        }

        public override bool CanAct(Entity entity) {
            return true;
        }
    }
}
