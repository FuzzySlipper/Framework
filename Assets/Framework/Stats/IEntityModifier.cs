﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct ModifiersChanged : IEntityMessage {
        public Entity Target;

        public ModifiersChanged(Entity target) {
            Target = target;
        }
    }
}
