using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PixelComrades {
    public partial class Vitals : GenericEnum<Vitals, int> {
        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }

    public partial class Attributes : GenericEnum<Attributes, int> {
        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }

    public partial class Skills : GenericEnum<Skills, int> {
        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }

    public partial class DamageTypes : GenericEnum<DamageTypes, int> {
        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }

    public partial class ItemRarity : GenericEnum<ItemRarity, int> {
        [Description("Special")] public const int Special = 10;
        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }
}
