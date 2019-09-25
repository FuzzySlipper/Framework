using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Experimental.PlayerLoop;

namespace PixelComrades {

    [Priority(Priority.Lower)]
    public class ActionSystem : SystemBase, IMainSystemUpdate {

        private NodeList<ActionUsingNode> _nodeList;

        public ActionSystem() {
            NodeFilter<ActionUsingNode>.New(ActionUsingNode.GetTypes());
        }

        public override void Dispose() {
            base.Dispose();
            if (_nodeList != null) {
                _nodeList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_nodeList == null) {
                _nodeList = EntityController.GetNodeList<ActionUsingNode>();
            }
            if (_nodeList != null) {
                _nodeList.Run(UpdateNode);
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
            if (action == null || !action.CanStart(owner) || owner.Tags.Contain(EntityTags.PerformingCommand)) {
                return false;
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
            node.ActionEvent.Action.ProcessCost(node.Entity);
            StopEvent(node);
        }

        public void StopEvent(ActionUsingNode node) {
            node.Entity.Tags.Remove(EntityTags.PerformingCommand);
            node.Stop();
        }
    }
}
