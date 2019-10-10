using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class EventLoadModel : IActionEventHandler {

        public string Data { get; }

        public EventLoadModel(string data) {
            Data = data;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            var model = ItemPool.Spawn(UnityDirs.Models, Data, Vector3.zero, Quaternion.identity);
            if (model != null) {
                var spawn = ae.Origin.Entity.Get<SpawnPivotComponent>();
                if (spawn != null) {
                    model.Transform.SetParentResetPos(ae.Origin.CurrentAction.Primary ? spawn.PrimaryPivot : spawn.SecondaryPivot);
                }
                else {
                    ae.Origin.Tr.SetParent(model.Transform);
                    model.Transform.ResetPos();
                }
                ae.Origin.CurrentAction.Entity.Add(new RenderingComponent(model.GetComponent<IRenderingComponent>()));
            }
        }
    }
}
