using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.High), AutoRegister]
    public class CollisionEventSystem : SystemBase, IReceiveGlobal<CollisionEvent>,
        IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent> {

        private List<ICollisionHandler> _globalHandlers = new List<ICollisionHandler>();
        
        private GameOptions.CachedBool _collisionMessage = new GameOptions.CachedBool("CollisionMessages");
        private FastString _collisionString = new FastString();
        private CircularBuffer<CollisionEvent> _eventLog = new CircularBuffer<CollisionEvent>(10, true);

        public CollisionEventSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(DespawnOnCollision)
            }));
        }

        [Command("printCollisionLog")]
        public static void PrintLog() {
            var log = World.Get<CollisionEventSystem>()._eventLog;
            foreach (var msg in log.InOrder()) {
                Console.Log(string.Format("{5}: {0} hit {1} at {2} {3} source {4}",
                    msg.Origin?.Entity.DebugId ?? "null",
                    msg.Target?.Entity.DebugId ?? "null",
                    msg.HitPoint, msg.HitNormal, msg.Source.DebugId, log.GetTime(msg)));
            }
        }

        public void AddGlobalCollisionHandler(ICollisionHandler collisionHandler) {
            _globalHandlers.Add(collisionHandler);
        }

        public void HandleGlobal(CollisionEvent msg) {
            _eventLog.Add(msg);
            if (msg.Hit < 0) {
                msg.Hit = CollisionResult.Hit;
                for (int h = 0; h < _globalHandlers.Count; h++) {
                    msg.Hit = MathEx.Min(_globalHandlers[h].CheckHit(msg), msg.Hit);
                }
            }
            if (msg.Hit <= 0) {
                return;
            }
            var criticalHit = msg.Target.Entity.Get<CriticalHitCollider>();
            if (criticalHit != null && criticalHit.IsCritical(msg.Target.Tr, msg.HitPoint)) {
                msg.Hit = CollisionResult.CriticalHit;
            }
            var origin = msg.Origin.Entity.FindTemplate<CharacterTemplate>();
            var target = msg.Target.Entity.FindTemplate<CharacterTemplate>();
            PostImpactEvent(origin, target, null, msg.HitPoint, msg.HitNormal  );
            if (!_collisionMessage) {
                return;
            }
            _collisionString.Clear();
            _collisionString.Append(msg.Origin.GetName());
            if (msg.Hit == CollisionResult.CriticalHit) {
                _collisionString.Append(" critically hit ");
            }
            else {
                _collisionString.Append(" struck ");
            }
            _collisionString.Append(msg.Target.GetName());
            MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(
                _collisionString.ToString(),"", Color.blue));
            if (msg.Target.Entity.HasComponent<DespawnOnCollision>()) {
                msg.Target.Entity.Destroy();
            }
        }

        public static void PostImpactEvent(CharacterTemplate origin, CharacterTemplate target, BaseActionTemplate action, Vector3 hitPoint, 
        Vector3 hitNormal) {
            var hitRotation = hitNormal == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(hitNormal);
            var ae = new ActionEvent(action, origin, target, hitPoint,hitRotation, ActionState.Impact);
            World.Get<RulesSystem>().Post(new ImpactEvent(action, origin, target, hitPoint, hitNormal));
            origin.Post(ae);
        }

        public void Handle(EnvironmentCollisionEvent msg) {
            if (msg.EntityHit.HasComponent<DespawnOnCollision>()) {
                msg.EntityHit.Destroy();
            }
        }

        public void Handle(PerformedCollisionEvent msg) {
            if (msg.Origin.Entity.HasComponent<DespawnOnCollision>()) {
                msg.Origin.Entity.Destroy();
            }
        }
    }

    public struct HitData {
        public CharacterTemplate Target { get; }
        public int Result { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }

        public HitData(int result, CharacterTemplate target, Vector3 point, Vector3 normal) {
            Result = result;
            Target = target;
            Point = point;
            Normal = normal;
        }

        public static implicit operator int(HitData reference) {
            return reference.Result;
        }
    }

    public struct ImpactEvent : IRuleEvent {
        public BaseActionTemplate Action { get; }
        public Entity Source { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public Vector3 Point { get { return Hit.Point; } }
        public Vector3 Normal { get { return Hit.Normal; } }
        public HitData Hit { get; }

        public ImpactEvent(CollisionEvent collisionEvent, BaseActionTemplate action, CharacterTemplate origin, CharacterTemplate target) {
            Source = collisionEvent.Source;
            Hit = new HitData(CollisionResult.Hit, target, collisionEvent.HitPoint, collisionEvent.HitNormal);
            Origin = origin;
            Target = target;
            Action = action;
        }

        public ImpactEvent(BaseActionTemplate action, CharacterTemplate origin, CharacterTemplate target, Vector3 hitPoint, Vector3 hitNormal) {
            Source = action.Entity;
            Hit = new HitData(CollisionResult.Hit, target, hitPoint, hitNormal);
            Origin = origin;
            Target = target;
            Action = action;
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
        public CollidableTemplate Origin { get; }
        public CollidableTemplate Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public int Hit { get; }
        public PerformedCollisionEvent(CollidableTemplate origin, CollidableTemplate target, Vector3 hitPoint, Vector3 hitNormal, int 
        hit = -1) {
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }
    }

    [Priority(Priority.Higher)]
    public struct CollisionEvent : IEntityMessage {
        public Entity Source { get; }
        public CollidableTemplate Origin { get; }
        public CollidableTemplate Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public int Hit;
        public CollisionEvent(Entity source, CollidableTemplate origin, CollidableTemplate target, Vector3 hitPoint, Vector3 hitNormal, int hit = -1) {
            Source = source;
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
