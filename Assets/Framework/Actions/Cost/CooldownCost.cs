using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CooldownCost : CommandCost {

        private Timer _cooldown;
        private bool _postUpdate;

        public float Length { get { return _cooldown.Length; } }

        public CooldownCost(float length, bool postUpdate) {
            _cooldown = new Timer(length, false);
            _postUpdate = postUpdate;
        }

        public override void ProcessCost(Entity entity) {
            _cooldown.Restart();
        }

        public override bool CanAct(Entity entity) {
            if (!_cooldown.IsActive) {
                return true;
            }
            if (_postUpdate) {
                entity.PostAll(new StatusUpdate("Still Recovering", Color.yellow));
            }
            return false;
        }
    }
}
