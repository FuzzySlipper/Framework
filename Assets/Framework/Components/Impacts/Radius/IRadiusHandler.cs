using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IRadiusHandler {
        void HandleRadius(Entity owner, Entity originalTarget);
    }
}
