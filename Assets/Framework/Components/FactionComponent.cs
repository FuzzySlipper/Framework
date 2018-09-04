using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct FactionComponent : IComponent {
        public int Faction { get; set; }
        public int Owner { get; set; }

        public FactionComponent(int faction, int owner) {
            Faction = faction;
            Owner = owner;
        }
    }
}
