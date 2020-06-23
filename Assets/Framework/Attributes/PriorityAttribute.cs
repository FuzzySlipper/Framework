using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PriorityAttribute : Attribute {
        public readonly int Value = 0;

        public PriorityAttribute(int value) {
            Value = value;
        }

        public PriorityAttribute(Priority value) {
            Value = (int) value;
        }
    }

    public class EnumLabelArrayAttribute : PropertyAttribute {
        public readonly System.Type Labels;

        public EnumLabelArrayAttribute(System.Type labels) {
            Labels = labels;
        }
    }

    public enum Priority {
        Lowest = 15,
        Lower = 10,
        Low = 8,
        Normal = 5,
        High = 2,
        Higher = 1,
        Highest = 0,
    }
}
