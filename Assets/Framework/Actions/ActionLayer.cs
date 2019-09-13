using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public abstract class ActionLayer : ISerializable {

        private CachedComponent<Action> _action;
        public List<IActionScriptedEvent> ScriptedEvents = new List<IActionScriptedEvent>();
        public Dictionary<string, IActionEvent> Events = new Dictionary<string, IActionEvent>();
        public bool IsMainLayer = false;

        public Action Action { get { return _action.Value; } }
        protected ActionLayer(Action action) {
            _action = new CachedComponent<Action>(action);
        }

        protected ActionLayer(SerializationInfo info, StreamingContext context) {
            _action = info.GetValue(nameof(_action), _action);
            ScriptedEvents = info.GetValue(nameof(ScriptedEvents), ScriptedEvents);
            Events = info.GetValue(nameof(Events), Events);
            IsMainLayer = info.GetValue(nameof(IsMainLayer), IsMainLayer);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_action), _action);
            info.AddValue(nameof(ScriptedEvents), ScriptedEvents);
            info.AddValue(nameof(Events), Events);
            info.AddValue(nameof(IsMainLayer), IsMainLayer);
        }

        public void PostAnimationEvent(ActionStateEvent stateEvent) {
            for (int i = 0; i < ScriptedEvents.Count; i++) {
                if (ScriptedEvents[i].Event == stateEvent.State) {
                    ScriptedEvents[i].Trigger(stateEvent);
                }
            }
        }

        public void PostAnimationEvent(ActionUsingNode node, string eventName) {
            if (Events.TryGetValue(eventName, out var animationEvent)) {
                animationEvent.Trigger(node, eventName);
            }
            ActionStateEvents state = AnimationEvents.ToStateEvent(eventName);
            if (state == ActionStateEvents.Activate) {
                node.Entity.Post(new ActionStateEvent(node.Entity, node.Entity, node.Animator.GetEventPosition, node.Animator.GetEventRotation, ActionStateEvents.Activate));
            }
            if (state == ActionStateEvents.None) {
                return;
            }
            var stateEvent = new ActionStateEvent(node.Entity, Action.GetEntity(), node.Animator.GetEventPosition, node.Animator.GetEventRotation, state);
            if (Action.Fx != null) {
                Action.Fx.TriggerEvent(stateEvent);
            }
            for (int i = 0; i < ScriptedEvents.Count; i++) {
                if (ScriptedEvents[i].Event != state) {
                    continue;
                }
                PostAnimationEvent(stateEvent);
                break;
            }
        }

        public virtual void Start(ActionUsingNode node) { }
        public abstract void Evaluate(ActionUsingNode node);
        public virtual void End(ActionUsingNode node) { }
    }
}
