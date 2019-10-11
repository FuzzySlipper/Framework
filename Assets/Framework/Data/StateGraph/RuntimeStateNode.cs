using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class RuntimeStateNode {
        public List<RuntimeConditionChecker> Conditions = new List<RuntimeConditionChecker>();
        public RuntimeStateNode LastEnter { get; protected set; }
        public RuntimeStateGraph Graph { get; }
        public StateGraphNode Node { get; }
        public float TimeEntered { get; protected set; }

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
            LastEnter = previous;
            TimeEntered = TimeManager.Time;
        }

        public virtual void OnExit() {}

        protected RuntimeStateNode GetOriginalNodeExit() {
            var outNode = Graph.OriginalGraph.GetConnectionEndpoint(Node.OutPoints[Node.DefaultExit]);
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
                    var exitNode = Graph.OriginalGraph.GetConnectionEndpoint(endPoint);
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
    }

    public interface IGlobalRuntimeStateNode {
        void CheckConditions();
    }
}
