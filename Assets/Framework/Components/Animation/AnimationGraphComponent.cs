using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class AnimationGraphComponent : IComponent, IDisposable {
        
        public RuntimeStateGraph Value { get; private set; }

        public AnimationGraphComponent(RuntimeStateGraph value) {
            Value = value;
        }

        public AnimationGraphComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }

        public static implicit operator RuntimeStateGraph(AnimationGraphComponent reference) {
            if (reference == null) {
                return null;
            }
            return reference.Value;
        }

        public void Dispose() {
            Value.Dispose();
            Value = null;
        }
    }
}
