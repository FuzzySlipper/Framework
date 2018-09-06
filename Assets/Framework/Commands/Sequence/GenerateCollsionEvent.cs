using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GenerateCollisionEvent : ICommandElement {

        public CommandSequence Owner { get; set; }
        public ActionStateEvents StateEvent { get; }

        public GenerateCollisionEvent(ActionStateEvents stateEvent) {
            StateEvent = stateEvent;
        }

        public void Start(Entity entity) {
            if (Owner.Target?.Target == null) {
                Owner.DefaultPostAdvance(this);
                return;
            }
            CollisionExtensions.GenerateHitLocDir(entity, Owner.Target.Target, out var hitpnt, out var dir);
            Owner.Target.Target.Post(new CollisionEvent(entity, Owner.Target.Target, hitpnt, dir, Owner.CurrentData));
            Owner.PostAdvance(Owner.Target.Target, hitpnt, Quaternion.Euler(dir), StateEvent);
        }
    }
}
