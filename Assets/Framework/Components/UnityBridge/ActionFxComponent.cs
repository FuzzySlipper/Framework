using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionFxComponent : IComponent, IReceive<ActionStateEvent>, IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent>, IReceive<CollisionEvent> {
        public ActionFx Fx { get; private set; }
        public int Owner { get; set; }

        public ActionFxComponent(ActionFx fx) {
            Fx = fx;
        }

        public void ChangeFx(ActionFx fx) {
            Fx = fx;
        }

        public void Handle(ActionStateEvent arg) {
            if (Fx != null) {
                Fx.TriggerEvent(arg);
            }
        }

        public void Handle(PerformedCollisionEvent arg) {
            if (Fx != null) {
                Fx.TriggerEvent(new ActionStateEvent(arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal), ActionStateEvents.Collision));
            }
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            if (Fx != null) {
                Fx.TriggerEvent(new ActionStateEvent(arg.EntityHit, null, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal), ActionStateEvents.Collision));
            }
        }

        public void Handle(CollisionEvent arg) {
            if (Fx != null) {
                Fx.TriggerEvent(new ActionStateEvent(arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal), ActionStateEvents.Collision));
            }
        }
    }
}
