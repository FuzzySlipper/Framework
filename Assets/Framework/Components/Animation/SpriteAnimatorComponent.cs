using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SpriteAnimatorComponent : IComponent {

        public SpriteAnimation CurrentAnimation;
        public int CurrentFrameIndex = 0;
        public Timer FrameTimer;
        public AnimationFrame CurrentFrame;
        
        public SpriteAnimatorComponent(){}
        
        public SpriteAnimatorComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
