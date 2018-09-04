using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EntityLevelComponent : IComponent {
        public int Owner { get; set; }
        public int Level;

        public EntityLevelComponent(int level) {
            Level = level;
        }
    }
}
