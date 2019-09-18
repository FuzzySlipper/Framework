using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PauseMovementForAnimation : IComponent {

        public string TargetClip;

        public PauseMovementForAnimation(SerializationInfo info, StreamingContext context) {
            TargetClip = info.GetValue(nameof(TargetClip), TargetClip);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(TargetClip), TargetClip);
        }
        
        public PauseMovementForAnimation(string targetClip) {
            TargetClip = targetClip;
        }
    }
}
