using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class EntityFlightSystem : SystemBase, IMainSystemUpdate, IReceive<CanMoveStatusChanged>, IReceive<ChangePositionEvent>  {

        private TemplateList<FlyingTemplate> _flyingList;
        private ManagedArray<FlyingTemplate>.RefDelegate _del;
        
        public EntityFlightSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(FlightMoveInput)
            }));
            _del = UpdateNode;
        }

        public override void Dispose() {
            base.Dispose();
            if (_flyingList != null) {
                _flyingList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_flyingList == null) {
                _flyingList = EntityController.GetTemplateList<FlyingTemplate>();
            }
            if (_flyingList != null) {
                _flyingList.Run(_del);
            }
        }

        private void UpdateNode(ref FlyingTemplate template) {
            if (template.SteeringInput != null && template.FlightMoveInput.CanMove) {
                template.FlightMoveInput.LookInputVector = template.SteeringInput.Look;
                template.FlightMoveInput.MoveInputVector = template.SteeringInput.Move;
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
            var node = arg.Target.GetTemplate<CollidableTemplate>();
            node.Entity.Post(new SetTransformPosition(node.Tr, arg.Position));
        }

        
    }
}
