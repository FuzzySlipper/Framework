using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EntityFlightSystem : SystemBase, IMainSystemUpdate, IReceive<CanMoveStatusChanged>, IReceive<ChangePositionEvent>  {

        private List<FlyingNode> _flyingList;
        
        public EntityFlightSystem() {
            EntityController.RegisterReceiver<FlightMoveInput>(this);
        }

        public override void Dispose() {
            base.Dispose();
            if (_flyingList != null) {
                _flyingList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_flyingList == null) {
                _flyingList = EntityController.GetNodeList<FlyingNode>();
            }
            if (_flyingList != null) {
                for (int i = 0; i < _flyingList.Count; i++) {
                    var node = _flyingList[i];
                    if (node.SteeringInput != null && node.FlightMoveInput.CanMove) {
                        node.FlightMoveInput.LookInputVector = node.SteeringInput.Look;
                        node.FlightMoveInput.MoveInputVector = node.SteeringInput.Move;
                    }
                }
            }
        }
        
        public void Handle(CanMoveStatusChanged arg) {
            var flightMoveInput = arg.Entity.Get<FlightMoveInput>();
            if (flightMoveInput == null) {
                return;
            }
            flightMoveInput.CanMove = arg.CanMove;
            if (!flightMoveInput.CanMove) {
                flightMoveInput.LookInputVector = flightMoveInput.MoveInputVector = Vector3.zero;
            }
        }

        public void Handle(ChangePositionEvent arg) {
            arg.Target.GetNode<CollidableNode>().Tr.position = arg.Position;
        }

        
    }
}
