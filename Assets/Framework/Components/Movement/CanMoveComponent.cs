using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class CanMoveComponent : IComponent {
        private ValueHolder<bool> _moveEnabled = new ValueHolder<bool>(true);

        public bool CanMove { get { return _moveEnabled.Value; } }
        public ValueHolder<bool> MoveEnabledHolder { get { return _moveEnabled; } }

        public CanMoveComponent() {
            _moveEnabled.OnResourceChanged += SendMessage;
        }

        private void SendMessage() {
            var entity = this.GetEntity();
            entity.Post(new CanMoveStatusChanged(CanMove, entity));
        }

        public CanMoveComponent(SerializationInfo info, StreamingContext context) {
            _moveEnabled = info.GetValue(nameof(_moveEnabled), _moveEnabled);
            _moveEnabled.OnResourceChanged += SendMessage;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_moveEnabled), _moveEnabled);
        }
    }

    public struct CanMoveStatusChanged : IEntityMessage {
        public bool CanMove { get; }
        public Entity Entity { get; }

        public CanMoveStatusChanged(bool canMove, Entity entity) {
            CanMove = canMove;
            Entity = entity;
        }
    }
}
