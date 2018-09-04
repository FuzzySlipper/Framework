using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AreaRadius : IRadiusHandler {

        public void HandleRadius(Entity owner, Entity originalTarget) {
            //List<Actor> hitActors = new List<Actor>();
            //if (alreadyHit != null) {
            //    hitActors.Add(alreadyHit);
            //}
            //var pnts = radius.RadiusPoints(center, OriginP3.GetTravelDirTo(center));
            //for (int i = 0; i < pnts.Count; i++) {
            //    var actor = Map.GetUnitAtCell(pnts[i]);
            //    if (actor == null || !TargettingType.IsValidTarget(Owner, actor)) {
            //        continue;
            //    }
            //    if (hitActors.Contains(actor)) {
            //        hitActors.Add(actor);
            //    }
            //}
            //for (int i = 0; i < hitActors.Count; i++) {
            //    if (hitActors[i] == alreadyHit) {
            //        continue;
            //    }
            //    ApplyRadiusCollision(hitActors[i], center, originalEvent);
            //}
        }
    }
}
