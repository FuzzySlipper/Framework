using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Experimental.PlayerLoop;

namespace PixelComrades {

    [Priority(Priority.Lower), AutoRegister]
    public class ActionSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<ActionEvent>, IReceive<AnimationEventTriggered> {

        private ManagedArray<ActionUsingTemplate>.RefDelegate _del;
        private TemplateList<ActionUsingTemplate> _templateList;
        private CircularBuffer<ActionEvent> _eventLog = new CircularBuffer<ActionEvent>(10, true);
        
        public ActionSystem() {
            _del = UpdateNode;
            TemplateFilter<ActionUsingTemplate>.Setup(ActionUsingTemplate.GetTypes());
            _templateList = EntityController.GetTemplateList<ActionUsingTemplate>();
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new [] {
                typeof(CurrentAction)
            }));
        }

        public override void Dispose() {
            base.Dispose();
            if (_templateList != null) {
                _templateList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _templateList.Run(_del);
        }

        public void HandleGlobal(ActionEvent arg) {
            _eventLog.Add(arg);
        }

        public void Handle(AnimationEventTriggered arg) {
            var node = arg.Entity.GetTemplate<ActionUsingTemplate>();
            if (node != null) {
                node.Current.Value.PostAnimationEvent(node, arg.Event);
            }
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

        private void UpdateNode(ref ActionUsingTemplate template) {
            if (template.CurrentState != ActionUsingTemplate.State.Disabled && template.Entity.IsDead()) {
                StopEvent(template);
                return;
            }
            if (template.CurrentState != ActionUsingTemplate.State.Disabled && template.ActionEvent.Current != null) {
                template.ActionEvent.Current.Evaluate(template);
            }
        }

        public bool Start(Entity owner, Transform spawn, Action action, Vector3 target) {
            if (owner == null || action == null || owner.Tags.Contain(EntityTags.PerformingAction)) {
                return false;
            }
            for (int i = 0; i < action.Costs.Count; i++) {
                if (!action.Costs[i].CanAct(owner)) {
                    return false;
                }
            }
            var node = owner.GetTemplate<ActionUsingTemplate>();
            if (node == null) {
                return false;
            }
            node.Entity.Tags.Add(EntityTags.PerformingAction);
            node.Entity.Post(new ActionEvent(node.Entity, node.Entity, node.Entity.GetPosition(), node.Entity.GetRotation(), ActionState.Start));
            node.Start(new ActionEvent(owner, spawn, action, target));
            return true;
        }

        private void Complete(ActionUsingTemplate template) {
            for (int i = 0; i < template.ActionEvent.Action.Costs.Count; i++) {
                template.ActionEvent.Action.Costs[i].ProcessCost(template.Entity);
            }
            StopEvent(template);
        }

        public void StopEvent(ActionUsingTemplate template) {
            template.Entity.Tags.Remove(EntityTags.PerformingAction);
            template.Stop();
        }
    }
}
