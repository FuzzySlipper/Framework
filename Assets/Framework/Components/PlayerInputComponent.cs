using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PlayerInputComponent : IComponent {
        public PlayerInput Input { get;}

        public PlayerInputComponent(PlayerInput input) {
            Input = input;
        }

        public PlayerInputComponent(SerializationInfo info, StreamingContext context) {
            Input = PlayerInput.main;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}
    }
}
