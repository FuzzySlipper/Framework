using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CostUse : CommandCost {

        public override void ProcessCost(Entity entity) {
            entity.Get<LimitedUses>(f => f.Use());
        }

        public override bool CanAct(Entity entity) {
            var uses = entity.Get<LimitedUses>();
            if (uses == null || uses.Current == 0) {
                return false;
            }
            return true;
        }
    }
}
