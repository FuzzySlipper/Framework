using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TeleportSequence : IActionEventHandler {
        public float Distance { get; }

        public TeleportSequence(float distance) {
            Distance = distance;
        }

        public void Trigger(ActionUsingTemplate template, string eventName) {
            template.Entity.Post(new ChangePositionEvent(template.Entity, FindPosition(template.Tr.position, template.Tr.forward, Distance)));
            //Player.Controller.Teleport(FindPosition(msg.Owner.Tr.position, msg.Owner.Tr.forward, current.Distance));
        }

        private Vector3 FindPosition(Vector3 start, Vector3 dir, float distance) {
            if (Physics.Raycast(start, dir, out var hit, distance, LayerMasks.WallsEnvironment)) {
                return hit.point;
            }
            return start + (dir.normalized * distance);
        }
    }
}
