using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpecialCamera : MonoSingleton<SpecialCamera> {

        [SerializeField] private Camera _cam = null;
        public Camera Cam { get => _cam; }
    }
}
