using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class ActionConfig : IComponent {

        [SerializeField] private Dictionary<string, List<IActionEventHandler>> _events = new Dictionary<string, List<IActionEventHandler>>();
        
        public List<ICommandCost> Costs = new List<ICommandCost>();
        public float Range;
        public string AnimationTrigger;
        public bool Primary;
        public int EquippedSlot = -1;
        public IActionConfig Source;
        public RuntimeStateGraph Graph;
        public SpriteAnimationReference Sprite;
        public ActionConfig() {}

        public ActionConfig(SerializationInfo info, StreamingContext context) {
            _events = info.GetValue(nameof(_events), _events);
            Costs = info.GetValue(nameof(Costs), Costs);
            Range = info.GetValue(nameof(Range), Range);
            Primary = info.GetValue(nameof(Primary), Primary);
            EquippedSlot = info.GetValue(nameof(EquippedSlot), EquippedSlot);
        }

        public void AddEvent(string eventName, IActionEventHandler eventHandler) {
            if (!_events.TryGetValue(eventName, out var list)) {
                list = new List<IActionEventHandler>();
                _events.Add(eventName, list);
            }
            list.Add(eventHandler);
        }

        public List<IActionEventHandler> GetEventHandler(string eventName) {
            return _events.TryGetValue(eventName, out var list) ? list : null;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_events), _events);
            info.AddValue(nameof(Costs), Costs);
            info.AddValue(nameof(Range), Range);
            info.AddValue(nameof(Primary), Primary);
            info.AddValue(nameof(EquippedSlot), EquippedSlot);
        }
    }
}
