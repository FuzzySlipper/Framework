using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PixelComrades {
    public partial class StatTypes : StringEnum<StatTypes> {
        public const string Vitals = "Vitals";
        public const string Attributes = "Attributes";
        public const string Skills = "Skills";
        public const string DamageTypes = "DamageTypes";
        public const string ItemRarity = "ItemRarity";
    }
}
