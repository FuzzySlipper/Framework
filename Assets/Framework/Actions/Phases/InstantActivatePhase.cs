using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class InstantActivatePhase : ActionPhases {
        
        public override bool CanResolve(ActionCommand cmd) {
            var target = cmd.Owner.Target.TargetChar != null ? cmd.Owner.Target.TargetChar : cmd.Owner;
            CollisionExtensions.GenerateHitLocDir(cmd.Owner.Tr, target.Tr, target.Collider, out var hitPoint, out var dir);
            var hitRot = Quaternion.LookRotation(dir);
            var hit = new HitData(CollisionResult.Hit, target, hitPoint, dir);
            cmd.ProcessHit(hit, hitRot);
            return true;
        }
    }
}
