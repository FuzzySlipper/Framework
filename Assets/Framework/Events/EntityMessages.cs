using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEntityMessage {}

    public class EntitySignals : GenericEnum<EntitySignals, int> {
        public const int TagsChanged = 0;
        public const int CharacterSetup = 100;
        public const int Moving = 102;
        public const int ReachedDestination = 103;
        public const int Rotated = 104;
        public const int Teleported = 105;
        public const int VisibilityChanged = 106;
        public const int ModsChanged = 107;
        public const int Dead = 108;
        public const int AttackMelee = 110;
        public const int AttackRange = 111;
        public const int CastSpell = 112;
        public const int UseAbility = 113;
        public const int ThrowItem = 114;
        public const int CommandChanged = 115;
        public const int CommandComplete = 116;
        public const int RaiseDead = 117;
        public const int EquipmentDetailsChanged = 118;
        public const int TurnReady = 119;
        public const int TurnEnded = 119;

        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }
}
