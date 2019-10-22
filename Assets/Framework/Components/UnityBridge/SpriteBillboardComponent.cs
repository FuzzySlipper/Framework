using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpriteBillboardComponent : IComponent {
        
        public DirectionsEight Orientation = DirectionsEight.Front;
        public float LastAngleHeight;
        public SpriteFacing Facing { get; }
        public bool Backwards { get; }
        public BillboardMode Billboard { get; }

        public SpriteBillboardComponent(SpriteFacing facing, bool backwards, BillboardMode billboard) {
            Facing = facing;
            Backwards = backwards;
            Billboard = billboard;
        }

        public SpriteBillboardComponent(SerializationInfo info, StreamingContext context) {
            Facing = info.GetValue(nameof(Facing), Facing);
            Billboard = info.GetValue(nameof(Billboard), Billboard);
            Backwards = info.GetValue(nameof(Backwards), Backwards);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Facing), Facing);
            info.AddValue(nameof(Billboard), Billboard);
            info.AddValue(nameof(Backwards), Backwards);
        }
    }
}
