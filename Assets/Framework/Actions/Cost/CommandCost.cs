using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [Serializable]
    public abstract class CommandCost : ICommandCost {
        public abstract void ProcessCost(Entity entity);
        public abstract bool CanAct(Entity entity);
    }
}
