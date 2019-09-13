using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public class ImpactRadius : IActionImpact, ISerializable {
        public ImpactRadiusTypes Radius { get; }

        [NonSerialized] private List<IActionImpact> _actions = new List<IActionImpact>();

        public ImpactRadius(ImpactRadiusTypes radius) {
            Radius = radius;
        }

        public float Power { get { return 0; } }
        public void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent) {
            _actions.Clear();
            _actions.AddRange(collisionEvent.Impacts);
            _actions.Remove(this);
            CollisionCheckSystem.OverlapSphere(collisionEvent.Origin, collisionEvent.Target, collisionEvent.HitPoint, Radius.ToFloat(), _actions);
        }

        public ImpactRadius(SerializationInfo info, StreamingContext context) {
            Radius = info.GetValue(nameof(Radius), Radius);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Radius), Radius);
        }
    }
}
