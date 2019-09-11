using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FakeFlightEngineConfig : ScriptableObject {
        [Header("Fake Flying")] 
        public FloatRange FakeFlightLimitX = new FloatRange(-200,200);
        public FloatRange FakeFlightLimitY = new FloatRange(0, 250);
        public float FakeFlightSpeed = 5f;
        public float FakeFlightStrafe = 5f;
        public float PitchSpeed = 10f;
        public float AngleOfPitch = 15f;
    }
}
