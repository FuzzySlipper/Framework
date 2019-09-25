using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EntityFlightSystem : SystemBase, IMainSystemUpdate, IReceive<CanMoveStatusChanged>, IReceive<ChangePositionEvent>  {

        private NodeList<FlyingNode> _flyingList;
        
        public EntityFlightSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(FlightMoveInput)
            }));
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
                _flyingList.Run(UpdateNode);
            }
        }

        private void UpdateNode(ref FlyingNode node) {
            if (node.SteeringInput != null && node.FlightMoveInput.CanMove) {
                node.FlightMoveInput.LookInputVector = node.SteeringInput.Look;
                node.FlightMoveInput.MoveInputVector = node.SteeringInput.Move;
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
            var node = arg.Target.GetNode<CollidableNode>();
            node.Entity.Post(new SetTransformPosition(node.Tr, arg.Position));
        }

        
    }
}
