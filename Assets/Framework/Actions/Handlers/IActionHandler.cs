using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IActionHandler {
        string TargetEvent { get; }
        void SetupEntity(Entity entity);
        void OnUsage(ActionEvent ae, ActionCommand cmd);
    }
}
