using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [System.Serializable]
    public abstract class ActionHandler : IActionHandler {
        
        [SerializeField, DropdownList(typeof(TargetEventTypes), "GetValues")] private string _targetEvent = "";
        
        public virtual string TargetEvent { get => _targetEvent; }
        public abstract void SetupEntity(Entity entity);
        public abstract void OnUsage(ActionEvent ae, ActionCommand cmd);
    }
}
