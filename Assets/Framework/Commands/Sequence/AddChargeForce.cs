using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AddChargeForce : ICommandElement {
        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }

        public AddChargeForce(ActionStateEvents stateEvent) {
            StateEvent = stateEvent;
        }

        public FloatRange ForceRange = new FloatRange(100, 4000);
        public float MaxChargeTime = 2;

        public void Start(Entity entity) {
            var charging = entity.Find<ActionTimer>();
            var transform = entity.Find<TransformComponent>().Tr;
            if (transform == null || charging == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var force = transform.forward * ForceRange.Lerp( Mathf.Clamp01(charging.ElapsedTime / MaxChargeTime));
            
            var rb = entity.Find<RigidbodyComponent>();
            if (rb != null) {
                entity.Post(new AddForceEvent(rb.Rb, force));
            }
            Owner.DefaultPostAdvance(this);
        }
    }
}
