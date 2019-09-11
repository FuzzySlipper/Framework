using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.High)]
    public class CollisionEventSystem : SystemBase, IReceiveGlobal<CollisionEvent> {

        public List<ICollisionHandler> Handlers = new List<ICollisionHandler>();

        public void HandleGlobal(CollisionEvent msg) {
            if (msg.Hit < 0) {
                msg.Hit = 10;
                for (int h = 0; h < Handlers.Count; h++) {
                    msg.Hit = MathEx.Min(Handlers[h].CheckHit(msg), msg.Hit);
                }
            }
            if (msg.Hit <= 0) {
                return;
            }
            var actionStateEvent = new ActionStateEvent(msg.Origin, msg.Target, msg.HitPoint, Quaternion.LookRotation(msg.HitNormal), ActionStateEvents.Impact);
            if (msg.Impacts == null) {
                Debug.LogFormat("{0} had no impacts {1}", msg.Origin.Name, System.Environment.StackTrace);
            }
            else {
                for (int j = 0; j < msg.Impacts.Count; j++) {
                    msg.Impacts[j].ProcessImpact(msg, actionStateEvent);
                }
            }
            msg.Origin.Post(actionStateEvent);
        }
    }

    [Priority(Priority.Higher)]
    public struct EnvironmentCollisionEvent : IEntityMessage {
        public Entity EntityHit;
        public Vector3 HitPoint;
        public Vector3 HitNormal;

        public EnvironmentCollisionEvent(Entity entityHit, Vector3 hitPoint, Vector3 hitNormal) {
            EntityHit = entityHit;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }

    [Priority(Priority.Higher)]
    public struct PerformedCollisionEvent : IEntityMessage {

        public Entity Origin;
        public Entity Target;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public int Hit;
        public List<IActionImpact> Impacts;

        public PerformedCollisionEvent(Entity origin, Entity target, Vector3 hitPoint, Vector3 hitNormal, List<IActionImpact> impacts, int hit = -1) {
            Impacts = impacts;
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }

        public PerformedCollisionEvent(Entity origin, Entity target, Vector3 hitPoint, Vector3 hitNormal, ActionImpacts impacts, int hit = -1) {
            Impacts = impacts?.Impacts;
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }
    }

    [Priority(Priority.Higher)]
    public struct CollisionEvent : IEntityMessage {

        public Entity Origin;
        public Entity Target;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public int Hit;
        public List<IActionImpact> Impacts;

        public CollisionEvent(Entity origin, Entity target, Vector3 hitPoint, Vector3 hitNormal, List<IActionImpact> impacts,  int hit = -1) {
            Impacts = impacts;
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }

        public CollisionEvent(Entity origin, Entity target, Vector3 hitPoint, Vector3 hitNormal, ActionImpacts impacts, int hit = -1) {
            Impacts = impacts?.Impacts;
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
