using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CooldownCost : CommandCost, ISerializable {

        private Timer _cooldown;
        private bool _postUpdate;

        public float Length { get { return _cooldown.Length; } }

        public CooldownCost(float length, bool postUpdate) {
            _cooldown = new Timer(length, false);
            _postUpdate = postUpdate;
        }

        public CooldownCost(SerializationInfo info, StreamingContext context) {
            _cooldown = info.GetValue(nameof(_cooldown), _cooldown);
            _postUpdate = info.GetValue(nameof(_postUpdate), _postUpdate);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_cooldown), _cooldown);
            info.AddValue(nameof(_postUpdate), _postUpdate);
        }

        public override void ProcessCost(Entity entity) {
            _cooldown.Restart();
        }

        public override bool CanAct(Entity entity) {
            if (!_cooldown.IsActive) {
                return true;
            }
            if (_postUpdate) {
                entity.PostAll(new StatusUpdate(entity,"Still Recovering", Color.yellow));
            }
            return false;
        }
    }
}
