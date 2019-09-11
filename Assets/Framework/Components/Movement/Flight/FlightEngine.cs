using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightEngine : ComponentBase {

        public Vector3 AvailableBoostForces;
        public Vector3 AvailableRotationForces;
        public Vector3 AvailableTranslationForces;
        public Vector3 MaxTranslationForces;
        public FlightEngineConfig Config;

        public FlightEngine(FlightEngineConfig config) {
            Config = config;
            RefreshEngine();
        }

        public void RefreshEngine() {
            AvailableTranslationForces = Config.DefaultTranslationForces;
            AvailableRotationForces = Config.DefaultRotationForces;
            AvailableBoostForces = Config.DefaultBoostForces;
            MaxTranslationForces = Config.DefaultTranslationForces * Config.MaxMulti;
        }
    }
}