using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class FlightEngine : IComponent {

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

        public FlightEngine(SerializationInfo info, StreamingContext context) {
            ItemPool.LoadAsset<FlightEngineConfig>(info.GetValue(nameof(Config), ""), a => Config = a);
            AvailableBoostForces = info.GetValue(nameof(AvailableBoostForces), AvailableBoostForces);
            AvailableRotationForces = info.GetValue(nameof(AvailableRotationForces), AvailableRotationForces);
            AvailableTranslationForces = info.GetValue(nameof(AvailableTranslationForces), AvailableTranslationForces);
            MaxTranslationForces = info.GetValue(nameof(MaxTranslationForces), MaxTranslationForces);
            RefreshEngine();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Config), ItemPool.GetAssetLocation(Config));
            info.AddValue(nameof(AvailableBoostForces), AvailableBoostForces);
            info.AddValue(nameof(AvailableRotationForces), AvailableRotationForces);
            info.AddValue(nameof(AvailableTranslationForces), AvailableTranslationForces);
            info.AddValue(nameof(MaxTranslationForces), MaxTranslationForces);
        }
    }
}