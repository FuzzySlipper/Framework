using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct TypeId : IComponent {
        public int Owner { get; set; }
        public string Id { get; }

        public TypeId(string id) : this() {
            Id = id;
        }

        public static implicit operator string(TypeId id) {
            return id.Id;
        }
    }
}
