using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventCheckRaycastCollision : IActionEventHandler {

        public ActionState State { get; }
        public float RayDistance { get; }
        public float RaySize { get; }
        public bool LimitToEnemy { get; }

        public EventCheckRaycastCollision(ActionState state, float rayDistance, float raySize, bool limit) {
            RayDistance = rayDistance;
            RaySize = raySize;
            State = state;
            LimitToEnemy = limit;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            var origin = ae.Origin;
            Vector3 originPos;
            Vector3 target;
            if (origin.Tags.Contain(EntityTags.Player)) {
                originPos = PlayerInputSystem.GetLookTargetRay.origin;
                target = PlayerInputSystem.GetMouseRaycastPosition(ae.Origin.CurrentAction.Action.Range);
            }
            else {
                originPos = ae.Position;
                target = ae.Origin.Target.GetPosition;
            }
            var actionEntity = origin.CurrentAction.Entity;
            var ray = new Ray(originPos, (target - originPos).normalized);
            CollisionEvent? ce = CollisionCheckSystem.Raycast(actionEntity, ray, RayDistance, LimitToEnemy);
            if (ce == null && RaySize > 0.01f) {
                ce = CollisionCheckSystem.SphereCast(actionEntity, ray, RayDistance, RaySize, LimitToEnemy);
            }
            if (ce != null) {
                var stateEvent = new ActionEvent(origin.Entity, ce.Value.Target.Entity, ce.Value.HitPoint, Quaternion.LookRotation(ce.Value
                .HitNormal), State);
                origin.Entity.Post(stateEvent);
            }
        }
    }
}
