using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DetermineHitOrMiss : ICommandElement {

        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }

        public DetermineHitOrMiss(ActionStateEvents stateEvent) {
            StateEvent = stateEvent;
        }

        public void Start(Entity entity) {
            if (Owner.Target?.Target == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            Owner.CurrentData = ToHitCalculation.Calculate(entity, Owner.Target.Target);
            Owner.DefaultPostAdvance(this);
        }
    }
}
