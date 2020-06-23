using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PixelComrades {
    public partial class PlayerControls : GenericEnum<PlayerControls, string> {
        
        public static string[] MoveButtons = new string[] {
            MovePosX, MoveNegX, MoveNegY, MovePosY
        };
        
        public const string MoveX = "MoveX";
        public const string MoveY = "MoveY";
        public const string LookX = "LookX";
        public const string LookY = "LookY";
        public const string MovePosX = "MoveX+";
        public const string MoveNegX = "MoveX-";
        public const string MovePosY = "MoveY+";
        public const string MoveNegY = "MoveY-";
        public const string Map = "Map";
        public const string Inventory = "Inventory";
        public const string Character = "Character";
        public const string Menu = "Menu";
        public const string Use = "Use";
        public const string Pause = "Pause";
        public const string ChangePrimary = "ChangePrimary";
        public const string ChangeSecondary = "ChangeSecondary";
        public const string ToggleFps = "ToggleFps";
        
        public override string Parse(string value, string defaultValue) {
            return value;
        }

        public static readonly Key[] NumericKeys = new[] {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0
        };

        public static readonly Key[] FunctionKeys = new[] {
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9
        };
    }
}
