using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CooldownCost : CommandCost {

        private Timer _cooldown;

        public float Length { get { return _cooldown.Length; } }

        public CooldownCost(float length) {
            _cooldown = new Timer(length, false);
        }

        public override void ProcessCost(Entity entity) {
            _cooldown.Activate();
        }

        public override bool CanAct(Entity entity) {
            if (!_cooldown.IsActive) {
                return true;
            }
            entity.Post(new StatusUpdate("Still Recovering", Color.yellow));
            return false;
        }
    }
}
