using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    [Priority(Priority.Lower), AutoRegister]
    public class ActionSystem : SystemBase, IReceiveGlobal<ActionEvent> {

        private CircularBuffer<ActionEvent> _eventLog = new CircularBuffer<ActionEvent>(10, true);
        
        public ActionSystem() {
        }

        public void HandleGlobal(ActionEvent arg) {
            _eventLog.Add(arg);
        }

        [Command("printActionStateLog")]
        public static void PrintLog() {
            var log = World.Get<ActionSystem>()._eventLog;
            foreach (var msg in log.InOrder()) {
                Console.Log(
                    string.Format(
                        "{0}: Action {1} source {2} target {3} at {4} {5} State {6}",
                        log.GetTime(msg),
                        msg.Action?.Entity.DebugId ?? "null",
                        msg.Origin?.Entity.DebugId ?? "null",
                        msg.Target?.Entity.DebugId ?? "null",
                        msg.Position, msg.Rotation, msg.State ));
            }
        }

        public void ProcessAnimationAction(AnimationEventTemplate aeTemplate, ActionTemplate action, AnimationEvent animEvent) {
            var character = aeTemplate.Entity.FindTemplate<CharacterTemplate>();
            var ae = new ActionEvent(character, character, aeTemplate.AnimEvent.Position,
                aeTemplate.AnimEvent.Rotation, AnimationEvents.ToStateEvent(animEvent));
            if (ae.State == ActionState.Activate) {
                Debug.DrawLine(
                    aeTemplate.AnimEvent.Position, aeTemplate.AnimEvent.Rotation.GetPosition(aeTemplate.AnimEvent.Position, 2.5f),
                    Color.red, 5f);
            }
            var eventName = animEvent.EventType.ToString();
            var animationList = action.Config.GetEventHandler(eventName);
            if (animationList != null) {
                for (int i = 0; i < animationList.Count; i++) {
                    animationList[i].Trigger(ae, eventName);
                }
            }
            if (ae.State != ActionState.None) {
                aeTemplate.Entity.Post(ae);
                action.Entity.PostNoGlobal(ae);
            }
            if (ae.State == ActionState.Start) {
                for (int i = 0; i < action.Config.Costs.Count; i++) {
                    action.Config.Costs[i].ProcessCost(ae.Origin, action.Entity);
                }
            }
        }
    }
}
