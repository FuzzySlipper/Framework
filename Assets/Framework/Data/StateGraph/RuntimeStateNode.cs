using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class RuntimeStateNode {
        public RuntimeStateNode LastEnter { get; protected set; }
        public RuntimeStateGraph Graph { get; }
        public float TimeEntered { get; protected set; }

        protected RuntimeStateNode(RuntimeStateGraph graph) {
            Graph = graph;
        }

        public virtual void OnEnter(RuntimeStateNode previous) {
            LastEnter = previous;
            TimeEntered = TimeManager.Time;
        }

        public virtual void OnExit() {
        }

        public abstract RuntimeStateNode GetExitNode();

        public abstract bool TryComplete(float dt);
    }

    public interface IGlobalRuntimeStateNode {
        void CheckConditions();
    }
}
