using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class Throwing {
        public static void Drop(Entity entity, Vector3 position) {
            //RaycastHit hit;
            //Ray ray = new Ray(position, Vector3.down);
            //if (Physics.Raycast(ray, out hit, 10, LayerMasks.Environment)) {
            //    position = hit.point;
            //}
            //var holder = ItemHolder.SpawnHolder(this);
            //holder.transform.position = position;
        }

        public static void Throw(Entity entity, Vector3 start) {
            //var owner = entity.Get<TransformComponent>();
            //var position = owner.Tr.position + owner.Tr.forward * Game.MapCellSize * 0.45f;
            //RaycastHit hit;
            //Ray ray = new Ray(owner.WorldCenter, Vector3.down);
            //if (Physics.Raycast(ray, out hit, 10, LayerMasks.Environment)) {
            //    position = hit.point;
            //}
            //var holder = ItemHolder.SpawnHolder(this);
            //holder.transform.position = position;
        }
    }

}
