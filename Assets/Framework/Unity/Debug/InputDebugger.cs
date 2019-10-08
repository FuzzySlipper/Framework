using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class InputDebugger : MonoBehaviour {
        public string[] CheckButtons = new string[0];

        void Update() {
            if (!Application.isPlaying || !DebugText.IsActive || !Game.GameActive) {
                return;
            }
            DebugText.UpdatePermText(
                "Input",
                string.Format("Time {0} Look {1} Move {2}",
                TimeManager.Time.ToString("F3"), PlayerInputSystem.LookInput, PlayerInputSystem.MoveInput));
        }
    }
}
