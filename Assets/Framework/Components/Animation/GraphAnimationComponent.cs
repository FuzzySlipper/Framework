using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class GraphAnimationComponent : IComponent {
        public GraphAnimation Animation { get; }
        public RuntimeAnimationGraph Graph { get; }

        public GraphAnimationComponent(GraphAnimation graphAnimation) {
            Animation = graphAnimation;
            Graph = graphAnimation.GetRuntimeGraph();
        }
        
        public GraphAnimationComponent(SerializationInfo info, StreamingContext context) {
            Animation = ItemPool.LoadAsset<GraphAnimation>(info.GetValue(nameof(Animation), ""));
            Graph = Animation.GetRuntimeGraph();
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Animation), ItemPool.GetAssetLocation(Animation));
        }
    }
}
