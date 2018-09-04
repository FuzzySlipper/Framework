using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class StatContainer : ComponentContainer<BaseStat> {
        protected StatContainer(BaseStat[] values) : base(values) {
        }
    }
}
