using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public interface IActionImpact {
        void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target);
    }

    public class ActionImpacts : GenericContainer<IActionImpact> {

        public ActionImpacts(IList<IActionImpact> values) : base(values) {}

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity target) {
            
            var owner = this.GetEntity();
            for (int i = 0; i < Count; i++) {
                this[i].ProcessAction(collisionEvent, stateEvent, owner, target);
            }
        }
        public ActionImpacts(){}

        public ActionImpacts Clone() {
            return new ActionImpacts(List.ToArray());
        }
    }
}
