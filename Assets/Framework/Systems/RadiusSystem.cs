using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class RadiusSystem : SystemBase, IRuleEventEnded<ImpactEvent>, IReceive<EnvironmentCollisionEvent> {
        public RadiusSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {typeof(ImpactRadius)}));
            World.Get<RulesSystem>().AddHandler<ImpactEvent>(this);
        }
        private Dictionary<int, IRadiusHandler> _radiusHandlers = new Dictionary<int, IRadiusHandler>();

        public void HandleRadius(Entity owner, Entity originalTarget, ImpactRadiusTypes radiusType) {
            if (_radiusHandlers.TryGetValue((int) radiusType, out var handler)) {
                handler.HandleRadius(owner, originalTarget);
            }
        }

        public void AddHandler(ImpactRadiusTypes radius, IRadiusHandler handler) {
            _radiusHandlers.Add((int)radius, handler);
        }
        
        public void RuleEventEnded(ref ImpactEvent context) {
            var component = context.Source.Find<ImpactRadius>();
            if (component == null) {
                return;
            }
            if (component.Radius == ImpactRadiusTypes.Single) {
                return;
            }
            // CollisionCheckSystem.OverlapSphere(
            //     context.Origin, context.Target, context.HitPoint,
            //     component.Radius.ToFloat(), component.LimitToEnemy);
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            var component = arg.EntityHit.Find<ImpactRadius>();
            if (component == null) {
                return;
            }
            // CollisionCheckSystem.OverlapSphere(arg.EntityHit, arg.EntityHit, arg.HitPoint,
            //     component.Radius.ToFloat(), component.LimitToEnemy);
        }
    }
}
