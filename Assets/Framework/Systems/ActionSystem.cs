using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Experimental.PlayerLoop;

namespace PixelComrades {

    [Priority(Priority.Lower), AutoRegister]
    public class ActionSystem : SystemBase, IMainSystemUpdate, IReceiveGlobal<ActionStateEvent> {

        private ManagedArray<ActionUsingNode>.RefDelegate _del;
        private NodeList<ActionUsingNode> _nodeList;
        private CircularBuffer<ActionStateEvent> _eventLog = new CircularBuffer<ActionStateEvent>(10, true);
        
        public ActionSystem() {
            _del = UpdateNode;
            NodeFilter<ActionUsingNode>.Setup(ActionUsingNode.GetTypes());
            _nodeList = EntityController.GetNodeList<ActionUsingNode>();
        }

        public override void Dispose() {
            base.Dispose();
            if (_nodeList != null) {
                _nodeList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _nodeList.Run(_del);
        }

        public void HandleGlobal(ActionStateEvent arg) {
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

        private void UpdateNode(ref ActionUsingNode node) {
            if (node.CurrentState != ActionUsingNode.State.Disabled && node.Entity.IsDead()) {
                StopEvent(node);
                return;
            }
            if (node.CurrentState != ActionUsingNode.State.Disabled && node.ActionEvent.Current != null) {
                node.ActionEvent.Current.Evaluate(node);
            }
        }

        public bool Start(Entity owner, Transform spawn, Action action, Vector3 target) {
            if (owner == null || action == null || owner.Tags.Contain(EntityTags.PerformingCommand)) {
                return false;
            }
            for (int i = 0; i < action.Costs.Count; i++) {
                if (!action.Costs[i].CanAct(owner)) {
                    return false;
                }
            }
            var node = owner.GetNode<ActionUsingNode>();
            if (node == null) {
                return false;
            }
            node.Entity.Tags.Add(EntityTags.PerformingCommand);
            node.Entity.Post(new ActionStateEvent(node.Entity, node.Entity, node.Entity.GetPosition(), node.Entity.GetRotation(), ActionStateEvents.Start));
            node.Start(new ActionEvent(owner, spawn, action, target));
            return true;
        }

        public void AdvanceEvent(ActionUsingNode node) {
            node.ActionEvent.Current.End(node);
            if (node.ActionEvent.IsLastIndex) {
                Complete(node);
                return;
            }
            node.ActionEvent = new ActionEvent(node.ActionEvent);
            //if (node.ActionEvent.Current.IsMainLayer) {
            //    node.Entity.Post(new ActionStateEvent(node.Entity, node.ActionEvent.ActionEntity, node.ActionEvent.SpawnPivot.position, node.ActionEvent.SpawnPivot.rotation, ActionStateEvents.Activate));
            //}
            node.ActionEvent.Current.Start(node);
        }

        private void Complete(ActionUsingNode node) {
            for (int i = 0; i < node.ActionEvent.Action.Costs.Count; i++) {
                node.ActionEvent.Action.Costs[i].ProcessCost(node.Entity);
            }
            StopEvent(node);
        }

        public void StopEvent(ActionUsingNode node) {
            node.Entity.Tags.Remove(EntityTags.PerformingCommand);
            node.Stop();
        }
    }
}
