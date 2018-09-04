using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IWorldItemInteraction {
        bool TryInteract(Entity item);
    }
}
