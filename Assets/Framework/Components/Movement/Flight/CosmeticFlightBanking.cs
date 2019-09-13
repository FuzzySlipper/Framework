using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class CosmeticFlightBanking : IComponent {

        private CachedTransform _bankTransform;
        public Transform BankTransform { get { return _bankTransform; } }
        public FlightControlConfig Config { get; }

        public CosmeticFlightBanking(Transform bankTransform, FlightControlConfig config) {
            _bankTransform = new CachedTransform(bankTransform);
            Config = config;
        }

        public CosmeticFlightBanking(SerializationInfo info, StreamingContext context) {
            _bankTransform = info.GetValue(nameof(_bankTransform), _bankTransform);
            Config = ItemPool.LoadAsset<FlightControlConfig>(info.GetValue(nameof(Config), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_bankTransform), _bankTransform);
            info.AddValue(nameof(Config), ItemPool.GetAssetLocation(Config));
        }
    }
}
