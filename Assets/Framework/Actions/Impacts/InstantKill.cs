using UnityEngine;

namespace PixelComrades {
    public class InstantKill : IActionImpact {

        private float _chance;

        public InstantKill(float chance) {
            _chance = chance;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            if (Game.DiceRollSuccess(_chance)) {
                target.Post(new DeathEvent(owner, target));
                target.Post(new CombatStatusUpdate("Lethal Hit!", Color.red));
            }
        }
    }
}