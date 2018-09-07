using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEntityMessage {}

    public class EntitySignals : GenericEnum<EntitySignals, int> {
        public const int TagsChanged = 0;
        public const int CharacterSetup = 1;
        public const int Moving = 2;
        public const int ReachedDestination = 3;
        public const int Rotated = 4;
        public const int Teleported = 5;
        public const int VisibilityChanged = 6;
        public const int ModsChanged = 7;
        public const int Dead = 8;
        public const int CommandChanged = 9;
        public const int CommandComplete = 10;
        public const int RaiseDead = 11;
        public const int CooldownTimerChanged = 12;
        public const int EquipmentDetailsChanged = 13;
        public const int TurnReady = 14;
        public const int TurnEnded = 15;

        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }
}
