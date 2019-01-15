using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ItemModifierPrefix : IComponent {
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

        public ItemModifierPrefix(DataEntry data) {
            Data = data;
        }
    }
}
