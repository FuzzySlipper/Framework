using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct AmountDescriptionComponent : IComponent {
        public int Owner { get; set; }
        public string Text;

    }
}
