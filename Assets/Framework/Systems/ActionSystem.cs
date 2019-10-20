using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Experimental.PlayerLoop;

namespace PixelComrades {

    [Priority(Priority.Lower), AutoRegister]
    public class ActionSystem : SystemBase, IReceiveGlobal<ActionEvent> {

        private CircularBuffer<ActionEvent> _eventLog = new CircularBuffer<ActionEvent>(10, true);
        
        public ActionSystem() {
            TemplateFilter<ActionUsingTemplate>.Setup();
            TemplateFilter<ActionTemplate>.Setup();
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
                        "{5}: Action source {0} target {1} at {2} {3} State {4}",
                        msg.Origin?.Entity.DebugId ?? "null",
                        msg.Target?.Entity.DebugId ?? "null",
                        msg.Position, msg.Rotation, msg.State, log.GetTime(msg)));
            }
        }

        public void ProcessAnimationAction(AnimationEventTemplate aeTemplate, ActionTemplate action, string animEvent) {
            var character = aeTemplate.Entity.FindTemplate<CharacterTemplate>();
            var ae = new ActionEvent(character, character, aeTemplate.AnimEvent.Position,
                aeTemplate.AnimEvent.Rotation, AnimationEvents.ToStateEvent(animEvent));
            if (ae.State == ActionState.Activate) {
                Debug.DrawLine(
                    aeTemplate.AnimEvent.Position, aeTemplate.AnimEvent.Rotation.GetPosition(aeTemplate.AnimEvent.Position, 2.5f),
                    Color.red, 5f);
            }
            var animationList = action.Config.GetEventHandler(animEvent);
            if (animationList != null) {
                for (int i = 0; i < animationList.Count; i++) {
                    animationList[i].Trigger(ae, animEvent);
                }
            }
            if (ae.State != ActionState.None) {
                aeTemplate.Entity.Post(ae);
                // are these already being triggered?
//                if (action.Fx != null) {
//                    action.Fx.Fx.TriggerEvent(ae);
//                }
            }
            if (ae.State == ActionState.Activate) {
                for (int i = 0; i < action.Config.Costs.Count; i++) {
                    action.Config.Costs[i].ProcessCost(ae.Origin, action.Entity);
                }
            }
        }
    }
}
