using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ToHitHandler : ICollisionHandler {

        public int CheckHit(CollisionEvent collisionEvent) {
            if (collisionEvent.Origin == null || collisionEvent.Target == null) {
                return CollisionResult.Hit;
            }
            //float hitRoll = Game.DiceRoll() + collisionEvent.ToHit;
            //var result = hitRoll - defendRoll;
            //RpgSystem.DistanceMulti(collisionEvent.OriginEvent.OriginP3, collisionEvent.PositionP3
            return ToHitCalculation.Calculate(collisionEvent.Origin.Entity, collisionEvent.Target.Entity);
        }
    }
}
