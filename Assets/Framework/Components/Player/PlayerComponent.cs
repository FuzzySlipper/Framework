using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PlayerComponent : IComponent {
        public int Owner { get; set; }
    }
}
