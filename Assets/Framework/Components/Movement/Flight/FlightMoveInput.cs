using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class FlightMoveInput : IComponent, IReceive<CanMoveStatusChanged>, IReceive<MoveInputMessage>, IReceive<ChangePositionEvent>, 
    IReceive<PhysicsInputMessage>{
        
        public Vector3 LookInputVector { get; private set; }
        public Vector3 MoveInputVector { get; private set; }
        public Vector3 InternalVelocityAdd { get; private set; }

        private bool _canMove = true;

        public void Handle(CanMoveStatusChanged arg) {
            _canMove = arg.CanMove;
            if (!_canMove) {
                LookInputVector = MoveInputVector = Vector3.zero;
            }
        }

        public void Handle(MoveInputMessage arg) {
            if (!_canMove) {
                return;
            }
            MoveInputVector = arg.Move;
            LookInputVector = arg.Look;
        }

        public void Handle(ChangePositionEvent arg) {
            this.GetEntity().GetNode<CollidableNode>().Tr.position = arg.Position;
        }

        public void Handle(PhysicsInputMessage arg) {
            InternalVelocityAdd += arg.Force;
        }

        public void UpdateControl(FlightControl control) {
            control.Thrust = MoveInputVector.z;
            control.Pitch = LookInputVector.y;
            control.Yaw = LookInputVector.x;
            control.StrafeHorizontal = MoveInputVector.x;
            control.StrafeVertical = MoveInputVector.y;
        }

        public FlightMoveInput() {}

        public FlightMoveInput(SerializationInfo info, StreamingContext context) {
            _canMove = info.GetValue(nameof(_canMove), _canMove);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_canMove), _canMove);
        }
    }
}
