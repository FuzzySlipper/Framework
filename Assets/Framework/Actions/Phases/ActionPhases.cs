using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
     public abstract class ActionPhases : ScriptableObject {
        public abstract bool CanResolve(ActionCommand cmd);
    }
}
