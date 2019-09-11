using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CanMoveComponent : ComponentBase {
        private ValueHolder<bool> _moveEnabled = new ValueHolder<bool>(true);

        public bool CanMove { get { return _moveEnabled.Value; } }
        public ValueHolder<bool> MoveEnabledHolder { get { return _moveEnabled; } }

        public CanMoveComponent() {
            _moveEnabled.OnResourceChanged += SendMessage;
        }

        private void SendMessage() {
            Entity.Post(new CanMoveStatusChanged(CanMove, Entity));
        }
    }

    public struct CanMoveStatusChanged : IEntityMessage {
        public bool CanMove;
        public Entity Entity;

        public CanMoveStatusChanged(bool canMove, Entity entity) {
            CanMove = canMove;
            Entity = entity;
        }
    }
}
