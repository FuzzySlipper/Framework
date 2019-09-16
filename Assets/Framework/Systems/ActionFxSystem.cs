using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionFxSystem : SystemBase, IReceiveGlobal<ActionStateEvent> {

        public void HandleGlobal(ActionStateEvent arg) {
            var data = arg.Origin.Entity.Find<ActionFxComponent>().Fx;
            if (data != null) {
                data.TriggerEvent(arg);
            }
        }
    }
}
