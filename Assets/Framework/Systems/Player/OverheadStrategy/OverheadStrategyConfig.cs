using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class OverheadStrategyConfig : PlayerControllerConfig {
        
        [SerializeField] private Transform _lookPivot = null;
        [SerializeField] private Camera _cam = null;
        [SerializeField] private RtsCameraConfig _rtsCameraConfig = null;

        public RtsCameraConfig RtsCameraConfig { get => _rtsCameraConfig; }
        public Camera Cam { get => _cam; }
        public Transform LookPivot { get => _lookPivot; }
    }
}
