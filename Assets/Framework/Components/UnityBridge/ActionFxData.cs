using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionFxData : IComponent, IReceive<ActionStateEvent> {
        public ActionFx Fx { get; }
        public int Owner { get; set; }

        public ActionFxData(ActionFx fx) {
            Fx = fx;
        }

        public void Handle(ActionStateEvent arg) {
            if (Fx != null) {
                Fx.TriggerEvent(arg);
            }
        }
    }
}
