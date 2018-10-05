using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ItemModifierSuffix : IComponent {
        private int _owner = -1;
        public int Owner {
            get {
                return _owner;
            }
            set {
                _owner = value;
            }
        }
        public DataEntry Data { get; }

        public ItemModifierSuffix(DataEntry data) {
            Data = data;
        }
    }
}
