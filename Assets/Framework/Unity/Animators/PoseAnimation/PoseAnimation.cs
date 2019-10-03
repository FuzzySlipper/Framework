using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XNode;

namespace PixelComrades {
    [CreateAssetMenu]
    public sealed class PoseAnimation : GraphAnimation {
        
        public List<Key> Keys = new List<Key>();
        
        
        public struct Key {
            public float Time;
            public AnimationCurve Curve;
            public MusclePose TargetPose;
            public MusclePose StartPose;
            public bool UseCurrent;
        }

        

        public class Node : XNode.Node {
            
        }
    }
    
}
