using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostAmmo : CommandCost, ISerializable {

        private CachedComponent<AmmoComponent> _ammoComponent;

        public CostAmmo(AmmoComponent ammoComponent) {
            _ammoComponent = new CachedComponent<AmmoComponent>(ammoComponent);
        }

        public CostAmmo(SerializationInfo info, StreamingContext context) {
            _ammoComponent = info.GetValue(nameof(_ammoComponent), _ammoComponent);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_ammoComponent), _ammoComponent);
        }

        public override void ProcessCost(ActionTemplate action, CharacterTemplate owner) {
            _ammoComponent.Value.Amount.ReduceValue(1);
        }

        public override bool CanAct(ActionTemplate action, CharacterTemplate owner) {
            if (!owner.IsPlayer()) {
                return true;
            }
            if (_ammoComponent.Value.Amount > 0) {
                return true;
            }
            owner.Post(new StatusUpdate(owner, "Not enough " + _ammoComponent.Value.Config.Name));
            return false;
        }
    }
}
