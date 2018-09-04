using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public abstract class CommandCost : ICommandCost {
        public abstract void ProcessCost(Entity entity);
        public abstract bool CanAct(Entity entity);
    }
}
