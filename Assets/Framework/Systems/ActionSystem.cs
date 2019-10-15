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
            TemplateFilter<ActionUsingTemplate>.Setup(ActionUsingTemplate.GetTypes());
        }

        public void HandleGlobal(ActionEvent arg) {
            if (arg.State == ActionState.Start) {
                arg.Origin.Tags.Set(EntityTags.PerformingAction, 1);
            }
            else if (arg.State == ActionState.Activate) {
                arg.Origin.Tags.Set(EntityTags.PerformingAction, 0);
            }
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
    }
}
