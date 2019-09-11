using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct FactionComponent : IComponent {

        private int _faction;

        public int Faction { get { return Owner < 0 ? -1 : _faction; } } 
        public int Owner { get; set; }

        public FactionComponent(int faction) : this() {
            _faction = faction;
        }

        public static implicit operator int(FactionComponent reference) {
            if (reference.Owner < 0) {
                return -1;
            }
            return reference.Faction;
        }
    }
}
