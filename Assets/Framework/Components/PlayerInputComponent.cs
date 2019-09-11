using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PlayerInputComponent : ComponentBase {
        public PlayerInput Input { get;}

        public PlayerInputComponent(PlayerInput input) {
            Input = input;
        }
    }
}
