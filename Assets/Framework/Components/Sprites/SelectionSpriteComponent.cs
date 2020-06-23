using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class SelectionSpriteComponent : IComponent {
        
        public SpriteRenderer Renderer;

        public SelectionSpriteComponent(SpriteRenderer renderer) { Renderer = renderer; }

        public SelectionSpriteComponent(){}
        
        public SelectionSpriteComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
