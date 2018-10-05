using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PixelComrades {

    public class Enums {
        
        private Dictionary<string, FakeEnum> _enums = new Dictionary<string, FakeEnum>();

        public FakeEnum this[string index] { get { return _enums.TryGetValue(index, out var fakeEnum) ? fakeEnum : null; } }

        public void Add(string name, FakeEnum fakeEnum) {
            _enums.SafeAdd(name, fakeEnum);
        }

        public bool TryGetEnumIndex(string fullName, out int index) {
            return GetEnumIndex(fullName, out index) != null;
        }

        public FakeEnum GetEnumIndex(string fullName, out int index) {
            FakeEnum fakeEnum = GetFakeEnum(fullName);
            if (fakeEnum == null) {
                index = -1;
                return null;
            }
            var splitName = fullName.Split('.');
            fakeEnum.TryParse(splitName.Length > 1 ? splitName[1] : fullName, out index);
            return fakeEnum;
        }

        public FakeEnum GetFakeEnum(string fullName) {
            var splitName = fullName.Split('.');
            if (splitName.Length < 2) {
                return _enums.TryGetValue(fullName, out var fullEnum) ? fullEnum : null;
            }
            return !_enums.TryGetValue(splitName[0], out var splitEnum) ? null : splitEnum;
        }
    }

    public static partial class EnumTypes {
        public const string Vitals = "Vitals";
        public const string Attributes = "Attributes";
        public const string DamageTypes = "DamageTypes";
        public const string ItemRarity = "ItemRarity";
        public const string EquipSlotTypes = "EquipSlotTypes";
        public const string ModifierGroups = "ModifierGroups";
        public const string ItemTypes = "ItemTypes";
    }
    
    public enum PlayerPronouns {
        [Description("he")] He,
        [Description("she")] She,
        [Description("they")] They
    }

    public enum MenuPivot {
        Center = 0,
        Right = 1,
        Left = 2,
        Icon = 3,
    }

    public enum Directions {
        Forward = 0,
        Right = 1,
        Back = 2,
        Left = 3,
        Up = 4,
        Down = 5,
        None = 99,
    }

    public enum DirectionsEight {
        Front = 0,
        FrontRight = 1,
        Right = 2,
        RearRight = 3,
        Rear = 4,
        RearLeft = 5,
        Left = 6,
        FrontLeft = 7,
        Top = 8,
        Bottom = 9,
    }

    public enum ActionDistance {
        Short = 0,
        Extended = 1,
        Medium = 2,
        Far = 3,
        Infinite = 4,
    }

}
