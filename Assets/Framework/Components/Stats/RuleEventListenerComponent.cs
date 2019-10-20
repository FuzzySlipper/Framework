using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class RuleEventListenerComponent : IComponent {

        public List<IRuleEventHandler> Handlers = new List<IRuleEventHandler>();

        /*
        public void Add(string eventName, IRuleEventHandler handler) {
            if (!_handlers.TryGetValue(eventName, out var list)) {
                list = _listPool.New();
                _handlers.Add(eventName, list);
            }
            list.Add(handler);
        }

        public void Remove(string eventName, IRuleEventHandler handler) {
            if (!_handlers.TryGetValue(eventName, out var list)) {
                return;
            }
            list.Remove(handler);
        }

        public bool TryGetValue(string eventName, out List<IRuleEventHandler> list) {
            return _handlers.TryGetValue(eventName, out list);
        }

        public void Dispose() {
            foreach (var list in _handlers) {
                _listPool.Store(list.Value);
            }
            _handlers.Clear();
        }
*/
        public RuleEventListenerComponent(){}
        
        public RuleEventListenerComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
