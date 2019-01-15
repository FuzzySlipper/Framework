using UnityEngine;
using System.Collections;

namespace PixelComrades {
    public class FpNormalState : FpState {
        public override Labels Label { get { return FpState.Labels.Normal; } }
    }
}