using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    
    [System.Serializable]
	public sealed class ActionConfig : IComponent {

        [SerializeField] private Dictionary<string, List<IActionEventHandler>> _events = new Dictionary<string, List<IActionEventHandler>>();
        
        public List<ICommandCost> Costs = new List<ICommandCost>();
<<<<<<< HEAD
        public List<ActionProviderEntry> Actions = new List<ActionProviderEntry>();
        public List<ActionPhases> Phases = new List<ActionPhases>();
        public List<IActionRequirement> Requirements = new List<IActionRequirement>();
        public string AnimationTrigger;
        public string Type;
        public string Focus;
        public int Range;
        public IActionConfig Source;
=======
        public List<IActionHandler> Actions = new List<IActionHandler>();
        public List<ActionPhases> Phases = new List<ActionPhases>();
        public List<IActionRequirement> Requirements = new List<IActionRequirement>();
        public string AnimationTrigger;
        public string TargetSlot;
        public int EquippedSlot = -1;
        public RuntimeStateGraph Graph;
        public SpriteAnimationReference Sprite;
        
        public string Focus;
>>>>>>> FirstPersonAction

        public ActionConfig() {}

        public IActionConfig Source { get; private set; }
        public int Range { get { return Source.Range; } }

        public ActionConfig(IActionConfig config) {
            Source = config;
        }

        public ActionConfig(SerializationInfo info, StreamingContext context) {
            _events = info.GetValue(nameof(_events), _events);
            Costs = info.GetValue(nameof(Costs), Costs);
<<<<<<< HEAD
            Type = info.GetValue(nameof(Type), Type);
=======
            EquippedSlot = info.GetValue(nameof(EquippedSlot), EquippedSlot);
>>>>>>> FirstPersonAction
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
<<<<<<< HEAD
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
=======
            info.AddValue(nameof(EquippedSlot), EquippedSlot);
>>>>>>> FirstPersonAction
        }

        public bool CanTarget(ActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            for (int i = 0; i < Requirements.Count; i++) {
                if (!Requirements[i].CanTarget(template, character, target)) {
                    return false;
                }
            }
            return true;
        }

        public bool CanEffect(ActionTemplate template, CharacterTemplate character, CharacterTemplate target) {
            for (int i = 0; i < Requirements.Count; i++) {
                if (!Requirements[i].CanEffect(template, character, target)) {
                    return false;
                }
            }
            return true;
        }
    }
}
