using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class TooltipComponent : IComponent {
        public TooltipComponent(){}
        public TooltipComponent(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }

    public struct TooltipDisplaying : IEntityMessage {
        public TooltipComponent Target { get; }

        public TooltipDisplaying(TooltipComponent target) {
            Target = target;
        }
    }
}
