using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class OverheadStrategyConfig : PlayerControllerConfig {
        [SerializeField] private Transform _lookPivot = null;
        public Transform LookPivot { get => _lookPivot; }
    }
}
