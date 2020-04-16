using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class ToHitCalculation  {
        
        private const int GrazeMin = 30;
        private const int GrazeMax = 50;
        private const int Critical = 100;

        public static int Calculate(Entity origin, Entity target) {
            var originStats = origin.Get<StatsContainer>();
            var targetStats = target.Get<StatsContainer>();
            if (origin == null || target == null || target == origin) {
                return CollisionResult.Hit;
            }
            if (target.Get<FactionComponent>().Faction == origin.Get<FactionComponent>().Faction) {
                return CollisionResult.Hit;
            }
            var defendTotal = targetStats.GetValue(Stats.Evasion);
            var attackTotal = 0;//originStats.GetValue(Stats.ToHit);
            //var finalAttack = Mathf.Clamp((attackTotal - defendTotal), 5, 100) + Game.Random.Next(1, 100);
            var finalAttack = Game.Random.Next(1, 100) + (attackTotal - defendTotal);
            var result = CollisionResult.Miss;
            if (finalAttack >= Critical - originStats.GetValue(Stats.CriticalHit)) {
                result = CollisionResult.CriticalHit;
            }
            else if (finalAttack >= GrazeMax) {
                result = CollisionResult.Hit;
            }
            else if (finalAttack >= GrazeMin) {
                result = CollisionResult.Graze;
            }
            //if (GameOptions.LogAllCommands) {
            //    DebugLogManager.Log(string.Format("{3} attacking {4} Atk {0} Def {1} Roll {2}", attackTotal, defendTotal, finalAttack, 
            //            collisionEvent.Owner != null? collisionEvent.Owner.Name : "null", collisionEvent.TargetUnit.Name), 
            //        string.Format("Result {0} Owner Pos {1} Target Pos {2}", result, collisionEvent.OriginEvent.OriginP3, collisionEvent.TargetUnit.GridPosition), LogType.Log);
            //}
            //if (GameOptions.LogCombat) {
            //    MessageKit<UINotificationWindow.Msg>.post(
            //        Messages.MessageLog, new UINotificationWindow.Msg(
            //            string.Format("{0} Rolled Attack {1:F0} {2} Rolled Defense {3:F0}, Combined: {4:F0} Result: {5}",
            //                collisionEvent.Owner != null ? collisionEvent.Owner.Name : "Null", attackTotal,
            //                collisionEvent.TargetUnit != null ? collisionEvent.TargetUnit.Name : "Null", defendTotal,
            //                finalAttack, result
            //                ), Color.yellow));
            //}
            //if (result == CollisionResult.Miss) {
            //    if (collisionEvent.TargetUnit != null) {
            //        collisionEvent.TargetUnit.AvoidedCollision(collisionEvent);
            //    }
            //}
            return result;
        }
    }
}
