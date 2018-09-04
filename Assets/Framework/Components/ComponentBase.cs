using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class ComponentBase : IComponent {
        public int Owner { get; set; }
    }
}
