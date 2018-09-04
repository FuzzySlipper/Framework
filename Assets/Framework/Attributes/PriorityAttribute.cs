using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PriorityAttribute : Attribute {
        public int Value = 0;

        public PriorityAttribute(int value) {
            Value = value;
        }

        public PriorityAttribute(Priority value) {
            Value = (int) value;
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
