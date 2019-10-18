using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class FakeFlightEngine : IComponent {

        public FakeFlightEngineConfig Config { get; private set; }

        public FakeFlightEngine(FakeFlightEngineConfig config) {
            Config = config;
        }

        public FakeFlightEngine(SerializationInfo info, StreamingContext context) {
            ItemPool.LoadAsset<FakeFlightEngineConfig>(info.GetValue(nameof(Config), ""), a => Config = a);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Config), ItemPool.GetAssetLocation(Config));
        }
    }
}
