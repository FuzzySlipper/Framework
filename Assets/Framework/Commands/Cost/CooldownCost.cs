using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CooldownCost : CommandCost {

        private Timer _cooldown;

        public CooldownCost(float length) {
            _cooldown = new Timer(length, false);
        }

        public override void ProcessCost(Entity entity) {
            _cooldown.Activate();
        }

        public override bool CanAct(Entity entity) {
            if (!_cooldown.IsActive) {
                return true;
            }
            entity.Get<StatusUpdateComponent>(e => e.Status = "Still Recovering");
            return false;
        }
    }
}
