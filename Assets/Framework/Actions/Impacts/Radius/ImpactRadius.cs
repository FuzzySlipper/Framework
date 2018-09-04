using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ImpactRadius : IComponent, IReceive<ActionStateEvent> {
        public int Owner { get; set; }
        public ImpactRadiusTypes Radius { get; }
        public bool Triggered { get; private set; }

        public ImpactRadius(ImpactRadiusTypes radius) {
            Radius = radius;
        }

        public void Handle(ActionStateEvent arg) {
            if (arg.State == ActionStateEvents.Start) {
                Triggered = false;
            }
            else if (arg.State == ActionStateEvents.AppliedImpact && !Triggered) {
                Triggered = true;
                World.Get<RadiusSystem>().HandleRadius(EntityController.GetEntity(arg.Origin), EntityController.GetEntity(arg.Focus), Radius);
            }
        }

    }
}
