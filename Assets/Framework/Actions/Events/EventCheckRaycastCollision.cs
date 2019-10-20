using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EventCheckRaycastCollision : IActionEventHandler {

        public float RayDistance { get; }
        public float RaySize { get; }
        public bool LimitToEnemy { get; }

        public EventCheckRaycastCollision(float rayDistance, float raySize, bool limit) {
            RayDistance = rayDistance;
            RaySize = raySize;
            LimitToEnemy = limit;
        }

        public void Trigger(ActionEvent ae, string eventName) {
            var origin = ae.Origin;
            Vector3 originPos;
            Vector3 target;
            if (origin.Tags.Contain(EntityTags.Player)) {
                originPos = PlayerInputSystem.GetLookTargetRay.origin;
                target = PlayerInputSystem.GetMouseRaycastPosition(ae.Action.Config.Range);
            }
            else {
                originPos = ae.Position;
                target = ae.Origin.Target.GetPosition;
            }
            var actionEntity = ae.Action.Entity;
            var ray = new Ray(originPos, (target - originPos).normalized);
            if (CollisionCheckSystem.Raycast(actionEntity, ray, RayDistance, LimitToEnemy) == null && RaySize > 0.01f) {
                CollisionCheckSystem.SphereCast(actionEntity, ray, RayDistance, RaySize, LimitToEnemy);
            }
        }
    }
}
