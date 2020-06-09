using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class ActionHandler : ScriptableObject, IActionHandler {
        public abstract void SetupEntity(Entity entity);
        public abstract void OnUsage(ActionEvent ae, ActionCommand cmd);
    }
}
