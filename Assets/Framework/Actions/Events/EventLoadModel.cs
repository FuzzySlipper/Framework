using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    public class EventDespawnModel : IActionEvent {
        public void Trigger(ActionUsingNode node, string eventName) {
            var model = node.ActionEvent.Action.Entity.Get<ModelComponent>();
            if (model != null) {
                ItemPool.Despawn(model.Model.Tr.gameObject);
                node.ActionEvent.Action.Entity.Remove(model);
            }
        }
    }

    public class EventLoadModel : IActionEvent {

        public string Data { get; }

        public EventLoadModel(string data) {
            Data = data;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            var model = ItemPool.Spawn(UnityDirs.Models, Data, Vector3.zero, Quaternion.identity);
            if (model != null) {
                model.transform.SetParentResetPos(node.ActionEvent.SpawnPivot != null? node.ActionEvent.SpawnPivot : node.Tr);
                node.ActionEvent.Action.Entity.Add(new ModelComponent(model.GetComponent<IModelComponent>()));
            }
        }
    }
}
