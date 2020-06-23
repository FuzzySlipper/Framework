using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class ActionConfig : IComponent {

        [SerializeField] private Dictionary<string, List<IActionEventHandler>> _events = new Dictionary<string, List<IActionEventHandler>>();
        
        public List<ICommandCost> Costs = new List<ICommandCost>();
        public List<ActionProviderEntry> Actions = new List<ActionProviderEntry>();
        public List<ActionPhases> Phases = new List<ActionPhases>();
        public List<IActionRequirement> Requirements = new List<IActionRequirement>();
        public string AnimationTrigger;
        public string Type;
        public string Focus;
        public int Range;
        public IActionConfig Source;

        public ActionConfig() {}

        public ActionConfig(SerializationInfo info, StreamingContext context) {
            _events = info.GetValue(nameof(_events), _events);
            Costs = info.GetValue(nameof(Costs), Costs);
            Type = info.GetValue(nameof(Type), Type);
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
            info.AddValue(nameof(Type), Type);
        }

        public bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            for (int i = 0; i < Requirements.Count; i++) {
                if (!Requirements[i].CanTarget(template, character, target)) {
                    return false;
                }
            }
            return true;
        }

        public bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            for (int i = 0; i < Requirements.Count; i++) {
                if (!Requirements[i].CanEffect(template, character, target)) {
                    return false;
                }
            }
            return true;
        }
    }
}
