using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CostAmmo : CommandCost {

        private AmmoComponent _ammoComponent;

        public CostAmmo(AmmoComponent ammoComponent) {
            _ammoComponent = ammoComponent;
        }

        public override void ProcessCost(Entity entity) {
            if (!entity.HasComponent<PlayerComponent>()) {
                return;
            }
            _ammoComponent.Amount.ReduceValue(1);
        }

        public override bool CanAct(Entity entity) {
            if (!entity.HasComponent<PlayerComponent>()) {
                return true;
            }
            if (_ammoComponent.Amount > 0) {
                return true;
            }
            entity.PostAll(new StatusUpdate("Not enough " + _ammoComponent.Template.Name));
            return false;
        }
    }
}
