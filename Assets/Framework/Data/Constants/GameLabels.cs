using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GameLabels : ScriptableSingleton<GameLabels> {

        [SerializeField] private string _currency = "Gold";

        public static string Currency { get => Main._currency; }
    }
}
