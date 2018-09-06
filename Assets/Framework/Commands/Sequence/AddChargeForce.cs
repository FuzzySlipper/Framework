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
            new AddForceEvent(entity.Find<RigidbodyComponent>().Rb, force).Post(entity);
            Owner.DefaultPostAdvance(this);
        }
    }
}
