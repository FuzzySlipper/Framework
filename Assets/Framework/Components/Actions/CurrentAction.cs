using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class CurrentAction : IComponent {
        
        private CachedComponent<Action> _current = new CachedComponent<Action>();
        
        public Action Value { get => _current; }
        
        public CurrentAction(){}

        public void SetAction(Action action) {
            if (action == null) {
                _current.Clear();
                return;
            }
            _current.Set(action);
        }
        
        public CurrentAction(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
