using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class EventLoadModel : IActionEventHandler {

        public GameObjectReference Data { get; }

        public EventLoadModel(GameObjectReference data) {
            Data = data;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            ItemPool.Spawn(Data,
                model => {
                    if (model != null) {
                        var spawn = ae.Origin.Entity.Get<SpawnPivotComponent>();
                        if (spawn != null) {
                            spawn.SetNewChild(model.Transform);
                        }
                        else {
                            ae.Origin.Tr.SetParent(model.Transform);
                            model.Transform.ResetPos();
                        }
                        ae.Action.Entity.Add(new RenderingComponent(model.GetComponent<IRenderingComponent>()));
                    }
                });
            
        }
    }
}
