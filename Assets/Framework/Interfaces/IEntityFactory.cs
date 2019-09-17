using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEntityFactory {
        bool TryStore(Entity entity);
    }
}
