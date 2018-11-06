using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ActionFxData : IComponent {
        public ActionFx Fx { get; }
        public int Owner { get; set; }

        public ActionFxData(ActionFx fx) : this() {
            Fx = fx;
        }
    }
}
