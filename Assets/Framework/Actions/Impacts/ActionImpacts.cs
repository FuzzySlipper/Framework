using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    public interface IActionImpact {
        float Power { get; }
        void ProcessImpact(CollisionEvent collisionEvent, ActionStateEvent stateEvent);
    }

    public sealed class ActionImpacts : IComponent {
        public List<IActionImpact> Impacts;

        public ActionImpacts(List<IActionImpact> values) : base() {
            Impacts = values;
        }
        
        public ActionImpacts(SerializationInfo info, StreamingContext context) {
            Impacts = info.GetValue(nameof(Impacts), Impacts);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Impacts), Impacts);
        }
    }
}
