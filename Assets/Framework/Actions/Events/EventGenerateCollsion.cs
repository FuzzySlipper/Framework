using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventGenerateCollisionEvent : IActionEventHandler {

        public void Trigger(ActionEvent ae, string eventName) {
            var entity = ae.Origin.Entity;
            var cmdTarget = entity.Get<CommandTarget>();
            Entity target;
            if (cmdTarget != null && cmdTarget.Target != null) {
                target = cmdTarget.Target;
            }
            else {
                target = ae.Origin.Entity;
            }
            if (target == null) {
                return;
            }
            var sourceNode = entity.FindTemplate<CollidableTemplate>();
            var targetNode = target.FindTemplate<CollidableTemplate>();
            if (sourceNode == null || targetNode == null) {
                return;
            }
            CollisionExtensions.GenerateHitLocDir(sourceNode.Tr, targetNode.Tr, targetNode.Collider, out var hitPoint, out var dir);
            var ce = new CollisionEvent(entity, sourceNode, targetNode, hitPoint, dir);
            target.Post(ce);
            entity.Post(new PerformedCollisionEvent(sourceNode, targetNode, ce.HitPoint, ce.HitNormal));
            var stateEvent = new ActionEvent(ae.Origin.Entity, ce.Target.Entity, ce.HitPoint, Quaternion.LookRotation(ce.HitNormal), 
            ActionState.Impact );
            ae.Origin.Entity.Post(stateEvent);
        }
    }
}
