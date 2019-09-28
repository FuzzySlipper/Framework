using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class EventSpawnProjectile : IActionEvent {

        public ActionStateEvents StateEvent { get; }
        public string Data { get; }

        public EventSpawnProjectile(ActionStateEvents stateEvent, string data) {
            StateEvent = stateEvent;
            Data = data;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            var target = node.ActionEvent.Target;
            Entity spawnEntity;
            if (node.ActionEvent.SpawnPivot != null) {
                spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(node.ActionEvent.Action.Entity, Data, target, 
                    node.ActionEvent.SpawnPivot.position, node.ActionEvent.SpawnPivot.rotation);
            }
            else {
                var animData = node.Animator;
                var spawnPos = animData?.GetEventPosition ?? (node.Tr != null ? node.Tr.position : Vector3.zero);
                var spawnRot = animData?.GetEventRotation ?? (node.Tr != null ? node.Tr.rotation : Quaternion.identity);
                DebugExtension.DebugPoint(spawnPos, Color.blue, 1f, 1f);
                spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(node.ActionEvent.Action.Entity, Data, node.ActionEvent.Target, 
                spawnPos, spawnRot);
            }
            if (spawnEntity != null) {
                if (node.ActionEvent.Action.Fx != null) {
                    var afx = spawnEntity.Get<ActionFxComponent>();
                    if (afx != null) {
                        afx.ChangeFx(node.ActionEvent.Action.Fx);
                    }
                    else {
                        spawnEntity.Add(new ActionFxComponent(node.ActionEvent.Action.Fx));
                    }
                }
            }
        }
    }
}
