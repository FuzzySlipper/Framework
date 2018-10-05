using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionFxSystem : SystemBase, IReceiveGlobal<ActionStateEvent> {

        public void HandleGlobal(ManagedArray<ActionStateEvent> list) {
            for (int i = 0; i < list.Count; i++) {
                var arg = list[i];
                EntityController.GetEntity(arg.Origin).Get<ActionFxData>(f => { f.Fx.TriggerEvent(arg); });
            }
        }
    }
}
