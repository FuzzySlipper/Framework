using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CosmeticFlightBanking : ComponentBase {

        public Transform BankTransform { get; }
        public FlightControlConfig Config { get; }

        public CosmeticFlightBanking(Transform bankTransform, FlightControlConfig config) {
            BankTransform = bankTransform;
            Config = config;
        }
    }
}
