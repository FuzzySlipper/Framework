using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventCheckRaycastCollision : IActionEvent {

        public ActionStateEvents StateEvent { get; }
        public float RayDistance { get; }
        public float RaySize { get; }
        public bool LimitToEnemy { get; }

        public EventCheckRaycastCollision(ActionStateEvents stateEvent, float rayDistance, float raySize, bool limit) {
            RayDistance = rayDistance;
            RaySize = raySize;
            StateEvent = stateEvent;
            LimitToEnemy = limit;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            var entity = node.Entity;
            Vector3 originPos;
            if (entity.Tags.Contain(EntityTags.Player)) {
                originPos = PlayerInput.GetTargetRay.origin;
            }
            else {
                var animData = entity.Find<AnimatorComponent>();
                originPos = animData?.Value?.GetEventPosition ?? (node.Tr != null ? node.Tr.position : Vector3.zero);
            }
            var target = node.ActionEvent.Target;
            var actionEntity = node.ActionEvent.Action.GetEntity();
            var ray = new Ray(originPos, (target - originPos).normalized);
            CollisionEvent? ce = CollisionCheckSystem.Raycast(actionEntity, ray, RayDistance, LimitToEnemy);
            if (ce == null && RaySize > 0.01f) {
                ce = CollisionCheckSystem.SphereCast(actionEntity, ray, RayDistance, RaySize, LimitToEnemy);
            }
            if (ce != null) {
                var stateEvent = new ActionStateEvent(node.Entity, ce.Value.Target.Entity, ce.Value.HitPoint, Quaternion.LookRotation(ce.Value.HitNormal), StateEvent);
                node.Entity.Post(stateEvent);
            }
        }
    }
}
