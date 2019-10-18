using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class CurrentAction : IComponent {

        private ActionTemplate _action;
        
        public ActionTemplate Value { get => _action; }
        
        public CurrentAction(){}

        public void SetAction(ActionTemplate action) {
            _action = action;
        }
        
        public CurrentAction(SerializationInfo info, StreamingContext context) {
            var entity = EntityController.GetEntity(info.GetValue(nameof(_action), -1));
            if (entity !=  null) {
                _action = entity.GetTemplate<ActionTemplate>();
            }
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_action), _action != null ? _action.Entity.Id : -1);
        }
    }
}
