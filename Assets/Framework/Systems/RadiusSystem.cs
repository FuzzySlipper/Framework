using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RadiusSystem : SystemBase {
        private Dictionary<int, IRadiusHandler> _radiusHandlers = new Dictionary<int, IRadiusHandler>();

        public void HandleRadius(Entity owner, Entity originalTarget, ImpactRadiusTypes radiusType) {
            if (_radiusHandlers.TryGetValue((int) radiusType, out var handler)) {
                handler.HandleRadius(owner, originalTarget);
            }
        }

        public void AddHandler(ImpactRadiusTypes radius, IRadiusHandler handler) {
            _radiusHandlers.Add((int)radius, handler);
        }
    }
}
