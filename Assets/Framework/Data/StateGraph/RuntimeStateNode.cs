using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class RuntimeStateNode {
        public List<RuntimeConditionChecker> Conditions = new List<RuntimeConditionChecker>();
        public RuntimeStateNode LastEnter { get; protected set; }
        public RuntimeStateGraph Graph { get; private set; }
        public StateGraphNode Node { get; private set; }
        public int Id { get { return Node.Id; } }
        public float TimeEntered { get; protected set; }
        public virtual string DebugInfo { get => ""; }

        protected RuntimeStateNode(StateGraphNode node, RuntimeStateGraph graph) {
            Node = node;
            Graph = graph;
            for (int i = 0; i < Node.Conditions.Count; i++) {
                var runtime = node.Conditions[i].GetRuntime();
                if (runtime != null) {
                    Conditions.Add(runtime);
                }
            }
        }

        public virtual void OnEnter(RuntimeStateNode previous) {
            if (!string.IsNullOrEmpty(Node.EnterEvent)){
                Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, Node.EnterEvent));
            }
            LastEnter = previous;
            TimeEntered = TimeManager.Time;
        }

        public virtual void OnExit() {
            if (!string.IsNullOrEmpty(Node.ExitEvent)) {
                Graph.Entity.Post(new AnimationEventTriggered(Graph.Entity, Node.ExitEvent));
            }
        }

        protected RuntimeStateNode GetOriginalNodeExit() {
            var outNode = Node.OutPoints[Node.DefaultExit].Target;
            return outNode != null ? Graph.GetRuntimeNode(outNode.Id) : null;
        }

        public bool HasTrueCondition() {
            for (int i = 0; i < Conditions.Count; i++) {
                if (Conditions[i].IsTrue(this)) {
                    return true;
                }
            }
            return false;
        }
        
        public RuntimeStateNode GetConditionExitNode() {
            for (int i = 0; i < Conditions.Count; i++) {
                var condition = Conditions[i];
                if (condition.IsTrue(this)) {
                    var endPoint = Node.OutPoints[condition.Original.Output];
                    var exitNode = endPoint.Target;
                    if (exitNode != null) {
                        return Graph.GetRuntimeNode(exitNode.Id);
                    }
                }
            }
            return GetOriginalNodeExit();
        }

        public virtual RuntimeStateNode GetExitNode() {
            return GetConditionExitNode();
        }

        public virtual bool TryComplete(float dt) {
            if (Node.AllowEarlyExit && HasTrueCondition()) {
                return true;
            }
            return false;
        }

        public virtual void Dispose() {
            Graph = null;
            Node = null;
        }
    }

    public interface IGlobalRuntimeStateNode {
        void CheckConditions();
    }
}
