using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class EventLoadModel : IActionEvent {

        public string Data { get; }

        public EventLoadModel(string data) {
            Data = data;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            var model = ItemPool.Spawn(UnityDirs.Models, Data, Vector3.zero, Quaternion.identity);
            if (model != null) {
                node.ParentSpawn(model.Transform);
                node.ActionEvent.Action.Entity.Add(new RenderingComponent(model.GetComponent<IRenderingComponent>()));
            }
        }
    }
}
