using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class RadiusSystem : SystemBase, IReceive<ImpactEvent>, IReceive<EnvironmentCollisionEvent> {
        public RadiusSystem() {
            EntityController.RegisterReceiver(
                new EventReceiverFilter(
                    this, new[] {
                        typeof(ImpactRadius)
                    }));
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

        public void Handle(ImpactEvent arg) {
            var component = arg.Source.Find<ImpactRadius>();
            if (component == null) {
                return;
            }
            CollisionCheckSystem.OverlapSphere(arg.Origin, arg.Target, arg.HitPoint, 
                component.Radius.ToFloat(),component.LimitToEnemy);
        }

        public void Handle(EnvironmentCollisionEvent arg) {
            var component = arg.EntityHit.Find<ImpactRadius>();
            if (component == null) {
                return;
            }
            CollisionCheckSystem.OverlapSphere(arg.EntityHit, arg.EntityHit, arg.HitPoint,
                component.Radius.ToFloat(), component.LimitToEnemy);
        }
    }
}
