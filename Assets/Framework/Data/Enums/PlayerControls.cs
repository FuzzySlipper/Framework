using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public partial class PlayerControls : GenericEnum<AnimationEvents, string> {
        
        public static string[] MoveButtons = new string[] {
            MovePosX, MoveNegX, MoveNegY, MovePosY
        };
        
        public const string MoveX = "MoveX";
        public const string MoveY = "MoveY";
        public const string LookX = "LookX";
        public const string LookY = "LookY";
        public const string Scroll = "ScrollAxis";
        public const string MovePosX = "MoveX+";
        public const string MoveNegX = "MoveX-";
        public const string MovePosY = "MoveY+";
        public const string MoveNegY = "MoveY-";
        public const string Map = "Map";
        public const string Cancel = "Cancel";
        public const string Inventory = "Inventory";
        public const string Character = "Character";
        public const string Menu = "Menu";
        public const string Use = "Use";
        public const string Pause = "Pause";
        
        public override string Parse(string value, string defaultValue) {
            return value;
        }

        public static readonly KeyCode[] NumericKeys = new[] {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8,
            KeyCode.Alpha9, KeyCode.Alpha0,
        };
    }
}
