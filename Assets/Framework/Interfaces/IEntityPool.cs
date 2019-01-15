using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEntityPool {
        void Store(Entity entity);
    }
}
