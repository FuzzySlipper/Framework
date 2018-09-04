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

        [SerializeField] private FloatRange _forceRange = new FloatRange(100, 4000);
        [SerializeField, Range(0, 3)] private float _maxChargeTime = 2;

        public void Start(Entity entity) {
            var charging = entity.Get<ActionTimer>();
            var transform = entity.GetSelfOrParent<TransformComponent>().Tr;
            if (transform == null || charging == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            var force = transform.forward * _forceRange.Lerp( Mathf.Clamp01(charging.ElapsedTime / _maxChargeTime));
            new AddForceEvent(entity.Get<RigidbodyComponent>().Rb, force).Post(entity);
            Owner.DefaultPostAdvance(this);
        }
    }
}
