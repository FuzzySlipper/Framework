using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public class EventSpawnProjectile : IActionEvent {

        public ActionStateEvents StateEvent { get; }
        public string Data;
        public List<IActionImpact> Impacts;

        public EventSpawnProjectile(ActionStateEvents stateEvent, List<IActionImpact> impacts, string data) {
            StateEvent = stateEvent;
            Impacts = impacts;
            Data = data;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            var target = node.ActionEvent.Target;
            Entity spawnEntity;
            if (node.ActionEvent.SpawnPivot != null) {
                spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(node.Entity, Data, target, node.ActionEvent.SpawnPivot.position, node.ActionEvent.SpawnPivot.rotation, Impacts);
            }
            else {
                var animData = node.Animator;
                var spawnPos = animData?.GetEventPosition ?? (node.Entity.Tr != null ? node.Entity.Tr.position : Vector3.zero);
                var spawnRot = animData?.GetEventRotation ?? (node.Entity.Tr != null ? node.Entity.Tr.rotation : Quaternion.identity);
                spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(node.Entity, Data, node.ActionEvent.Target, spawnPos, spawnRot, Impacts);
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
