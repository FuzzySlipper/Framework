using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
     public abstract class ActionPhases  {
        public abstract bool CanResolve(ActionCommand cmd);
    }
}
