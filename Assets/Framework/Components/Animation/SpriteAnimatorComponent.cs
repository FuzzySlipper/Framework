using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpriteAnimatorComponent : IComponent {

        public SpriteAnimation CurrentAnimation;
        public int FrameIndex = 0;
        public AnimationFrame CurrentFrame;
        public float FrameTimer;
        
        public SpriteAnimatorComponent(){}
        
        public SpriteAnimatorComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
