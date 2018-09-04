using UnityEngine;

namespace PixelComrades {
    public class InstantKill : IActionImpact {

        private float _chance;
        private ActionFx _actionFx;
        //private int _damageType;

        public InstantKill(float chance, ActionFx actionFx) {
            _chance = chance;
            _actionFx = actionFx;
        }

        public void ProcessAction(CollisionEvent collisionEvent, ActionStateEvent stateEvent, Entity owner, Entity target) {
            if (collisionEvent.Hit <= 0) {
                return;
            }
            if (_actionFx != null) {
                _actionFx.TriggerEvent(stateEvent);
            }
            if (Game.DiceRollSuccess(_chance)) {
                new DeathEvent(owner, target).Post(target);
                new FloatingTextMessage("Lethal Hit!", Color.red, target).Post(target);
            }
        }
    }
}