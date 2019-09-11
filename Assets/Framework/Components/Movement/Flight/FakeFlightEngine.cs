using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FakeFlightEngine : ComponentBase {

        public FakeFlightEngineConfig Config { get; }

        public FakeFlightEngine(FakeFlightEngineConfig config) {
            Config = config;
        }
    }
}
