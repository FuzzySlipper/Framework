using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TooltipComponent : IComponent {
        public int Owner { get; set; }
        public Action<IComponent> OnTooltipDel;

        public void Tooltip() {
            if (OnTooltipDel != null) {
                OnTooltipDel(this);
            }
        }
    }
}
