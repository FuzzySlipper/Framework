using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.High)]
    public class CollisionEventSystem : SystemBase, IReceiveGlobal<CollisionEvent> {

        public List<ICollisionHandler> Handlers = new List<ICollisionHandler>();

        public void HandleGlobal(ManagedArray<CollisionEvent> arg) {
            for (int i = 0; i < arg.Count; i++) {
                var msg = arg[i];
                if (msg.Hit < 0) {
                    msg.Hit = 10;
                    for (int h = 0; h < Handlers.Count; h++) {
                        msg.Hit = MathEx.Min(Handlers[h].CheckHit(msg), msg.Hit);
                    }
                }
                if (msg.Hit <= 0) {
                    continue;
                }
                msg.Target.Post(new ActionStateEvent(msg.Origin.Id, msg.Target.Id, msg.HitPoint, Quaternion.LookRotation(msg.HitNormal), ActionStateEvents.Collision));
                var actionStateEvent = new ActionStateEvent(msg.Origin.Id, msg.Target.Id, msg.HitPoint, Quaternion.LookRotation(msg.HitNormal), ActionStateEvents.AppliedImpact);
                msg.Origin.Find<ActionImpacts>(c => c.ProcessAction(msg, actionStateEvent, msg.Target));
                msg.Origin.Post(actionStateEvent);
            }
        }
    }

    [Priority(Priority.Higher)]
    public struct CollisionEvent : IEntityMessage {

        public Entity Origin;
        public Entity Target;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public int Hit;

        public CollisionEvent(Entity origin, Entity target, Vector3 hitPoint, Vector3 hitNormal, int hit = -1) {
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }
    }

    public interface ICollisionHandler {
        int CheckHit(CollisionEvent collisionEvent);
    }

    public class CollisionResult : GenericEnum<CollisionResult, int> {
        public static int Miss = 0;
        public static int Graze = 5;
        public static int Hit = 10;
        public static int CriticalHit = 15;

        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }
}
