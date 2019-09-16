using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class TeleportSequence : IActionEvent {
        public float Distance { get; }

        public TeleportSequence(float distance) {
            Distance = distance;
        }

        public void Trigger(ActionUsingNode node, string eventName) {
            node.Entity.Post(new ChangePositionEvent(FindPosition(node.Tr.position, node.Tr.forward, Distance)));
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
