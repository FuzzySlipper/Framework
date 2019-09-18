using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.High)]
    public class CollisionEventSystem : SystemBase, IReceiveGlobal<CollisionEvent>, 
        IReceiveGlobal<EnvironmentCollisionEvent>, IReceiveGlobal<PerformedCollisionEvent> {

        public List<ICollisionHandler> Handlers = new List<ICollisionHandler>();
        
        private GameOptions.CachedBool _collisionMessage = new GameOptions.CachedBool("CollisionMessages");
        private FastString _collisionString = new FastString();

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
            var actionStateEvent = new ActionStateEvent(msg.Origin.Entity, msg.Target.Entity, msg.HitPoint, Quaternion.LookRotation(msg
            .HitNormal), 
            ActionStateEvents.Impact);
            if (msg.Impacts == null) {
                Debug.LogFormat("{0} had no impacts {1}", msg.Origin.GetName(), System.Environment.StackTrace);
            }
            else {
                for (int j = 0; j < msg.Impacts.Count; j++) {
                    msg.Impacts[j].ProcessImpact(msg, actionStateEvent);
                }
            }
            msg.Origin.Post(actionStateEvent);
            if (!_collisionMessage) {
                return;
            }
            _collisionString.Clear();
            _collisionString.Append(msg.Origin.GetName());
            _collisionString.Append(" struck ");
            _collisionString.Append(msg.Target.GetName());
            MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(
                _collisionString.ToString(),"", Color.blue));
            if (msg.Target.Entity.HasComponent<DespawnOnCollision>()) {
                msg.Target.Entity.Destroy();
            }
        }

        public void HandleGlobal(EnvironmentCollisionEvent msg) {
            if (msg.EntityHit.HasComponent<DespawnOnCollision>()) {
                msg.EntityHit.Destroy();
            }
        }

        public void HandleGlobal(PerformedCollisionEvent msg) {
            if (msg.Origin.Entity.HasComponent<DespawnOnCollision>()) {
                msg.Origin.Entity.Destroy();
            }
        }
    }

    [Priority(Priority.Higher)]
    public struct EnvironmentCollisionEvent : IEntityMessage {
        public Entity EntityHit { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }

        public EnvironmentCollisionEvent(Entity entityHit, Vector3 hitPoint, Vector3 hitNormal) {
            EntityHit = entityHit;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }

    [Priority(Priority.Higher)]
    public struct PerformedCollisionEvent : IEntityMessage {

        public CollidableNode Origin { get; }
        public CollidableNode Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public int Hit { get; }
        public List<IActionImpact> Impacts { get; }

        public PerformedCollisionEvent(CollidableNode origin, CollidableNode target, Vector3 hitPoint, Vector3 hitNormal, List<IActionImpact> impacts, int hit = -1) {
            Impacts = impacts;
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }

        public PerformedCollisionEvent(CollidableNode origin, CollidableNode target, Vector3 hitPoint, Vector3 hitNormal, ActionImpacts impacts, int hit = -1) {
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

        public CollidableNode Origin;
        public CollidableNode Target;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public int Hit;
        public List<IActionImpact> Impacts;

        public CollisionEvent(CollidableNode origin, CollidableNode target, Vector3 hitPoint, Vector3 hitNormal, List<IActionImpact> impacts,  int hit = 
        -1) {
            Impacts = impacts;
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }

        public CollisionEvent(CollidableNode origin, CollidableNode target, Vector3 hitPoint, Vector3 hitNormal, ActionImpacts impacts, int hit = -1) {
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
