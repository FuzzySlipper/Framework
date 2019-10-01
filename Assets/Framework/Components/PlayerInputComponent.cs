using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PlayerInputComponent : IComponent {
        
        public IPlayerInputHandler Handler;

        public PlayerInputComponent(IPlayerInputHandler input) {
            Handler = input;
        }

        public PlayerInputComponent(SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
