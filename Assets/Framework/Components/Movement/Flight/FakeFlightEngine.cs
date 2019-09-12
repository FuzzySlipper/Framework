using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class FakeFlightEngine : IComponent {

        public FakeFlightEngineConfig Config { get; }

        public FakeFlightEngine(FakeFlightEngineConfig config) {
            Config = config;
        }

        public FakeFlightEngine(SerializationInfo info, StreamingContext context) {
            Config = ItemPool.LoadAsset<FakeFlightEngineConfig>(info.GetValue(nameof(Config), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Config), ItemPool.GetAssetLocation(Config));
        }
    }
}
