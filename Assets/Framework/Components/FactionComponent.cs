using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public struct FactionComponent : IComponent {

        private int _faction;

        public int Faction { get { return _faction; } } 

        public FactionComponent(int faction) : this() {
            _faction = faction;
        }

        public static implicit operator int(FactionComponent reference) {
            return reference.Faction;
        }

        public FactionComponent(SerializationInfo info, StreamingContext context) {
            _faction = info.GetValue(nameof(_faction), 0);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_faction), _faction);
        }
    }
}
