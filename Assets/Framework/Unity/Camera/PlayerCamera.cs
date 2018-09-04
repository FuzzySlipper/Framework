using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public partial class PlayerCamera : MonoSingleton<PlayerCamera> {

        public static Camera Cam { get; private set; }

        void Awake() {
            Cam = GetComponent<Camera>();
        }
    }
}
