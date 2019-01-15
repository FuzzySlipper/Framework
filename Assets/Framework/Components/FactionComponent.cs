using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct FactionComponent : IComponent {
        public int Faction { get; }
        public int Owner { get; set; }

        public FactionComponent(int faction) : this() {
            Faction = faction;
        }

        public static implicit operator int(FactionComponent reference) {
            return reference.Faction;
        }
    }
}
