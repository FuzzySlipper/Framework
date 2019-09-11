using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class ActionLayer {

        public Action Action { get; }
        public List<IActionScriptedEvent> ScriptedEvents = new List<IActionScriptedEvent>();
        public Dictionary<string, IActionEvent> Events = new Dictionary<string, IActionEvent>();
        public bool IsMainLayer = false;

        protected ActionLayer(Action action) {
            Action = action;
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
