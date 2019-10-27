using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class UnityAnimatorComponent : IComponent {

        private CachedUnityComponent<Animator> _animator;

        public Animator Value { get { return _animator; } }
        
        public UnityAnimatorComponent(Animator animator) {
            _animator = new CachedUnityComponent<Animator>(animator);
        }
        
        public UnityAnimatorComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
