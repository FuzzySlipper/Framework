using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventCheckRaycastCollision : IActionEvent {

        public ActionStateEvents StateEvent { get; }
        public List<IActionImpact> Impacts;
        public float RayDistance;
        public float RaySize;
        public bool LimitToEnemy;

        public EventCheckRaycastCollision(ActionStateEvents stateEvent, List<IActionImpact> impacts, float rayDistance, float raySize, bool limit) {
            Impacts = impacts;
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
                var animData = entity.Find<AnimatorData>();
                originPos = animData?.Animator?.GetEventPosition ?? (entity.Tr != null ? entity.Tr.position : Vector3.zero);
            }
            var target = node.ActionEvent.Target;
            var actionEntity = node.ActionEvent.Action.Entity;
            var ray = new Ray(originPos, (target - originPos).normalized);
            CollisionEvent? ce = CollisionCheckSystem.Raycast(actionEntity, ray, RayDistance, LimitToEnemy, Impacts);
            if (ce == null && RaySize > 0.01f) {
                ce = CollisionCheckSystem.SphereCast(actionEntity, ray, RayDistance, RaySize, LimitToEnemy, Impacts);
            }
            if (ce != null) {
                var stateEvent = new ActionStateEvent(node.Entity, ce.Value.Target, ce.Value.HitPoint, Quaternion.LookRotation(ce.Value.HitNormal), StateEvent);
                node.Entity.Post(stateEvent);
            }
        }
    }
}
