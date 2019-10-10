using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class RuntimeStateNode {
        public RuntimeStateNode LastEnter { get; protected set; }
        public RuntimeStateGraph Graph { get; }
        public StateGraphNode Node { get; }
        public float TimeEntered { get; protected set; }
        public bool BlocksGlobal { get => Node.BlockAnyStateChecks; }

        protected RuntimeStateNode(StateGraphNode node, RuntimeStateGraph graph) {
            Node = node;
            Graph = graph;
        }

        public virtual void OnEnter(RuntimeStateNode previous) {
            LastEnter = previous;
            TimeEntered = TimeManager.Time;
        }

        public virtual void OnExit() {}

        protected RuntimeStateNode GetOriginalNodeExit() {
            var outNode = Graph.OriginalGraph.GetConnectionEndpoint(Node.OutPoints[0]);
            return outNode != null ? Graph.GetRuntimeNode(outNode.Id) : null;
        }

        public abstract RuntimeStateNode GetExitNode();

        public abstract bool TryComplete(float dt);
    }

    public interface IGlobalRuntimeStateNode {
        void CheckConditions();
    }
}
