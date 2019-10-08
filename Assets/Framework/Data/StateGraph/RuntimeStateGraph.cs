using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class RuntimeStateGraph {
        
        private Dictionary<int, RuntimeStateNode> _lookup = new Dictionary<int, RuntimeStateNode>();
        private Dictionary<string, GraphTrigger> _triggers = new Dictionary<string, GraphTrigger>();

        public float TimeStartGraph { get; protected set; }
        public RuntimeStateNode Current { get; protected set; }
        public RuntimeStateNode StartNode { get; protected set; }

        public bool IsActive { get { return Current != null; } }
        public StateGraph OriginalGraph { get; private set; }
        public Entity Owner { get; private set; }

        public RuntimeStateGraph(StateGraph graph) {
            OriginalGraph = graph;
            for (int i = 0; i < graph.Triggers.Count; i++) {
                _triggers.AddOrUpdate(graph.Triggers[i].Key, graph.Triggers[i]);
            }
            for (int i = 0; i < graph.Count; i++) {
                CreateRuntimeNode(graph[i]);
            }
            StartNode = GetRuntimeNode(graph.Default != null ? graph.Default.Id : graph[0].Id);
        }
        
        public void SetOwner(Entity owner) {
            Owner = owner;
        }

        public virtual void Start() {
            TimeStartGraph = TimeManager.Time;
            SetCurrentNode(StartNode);
        }

        public virtual void Update(float dt) {
            if (Current.TryComplete(dt)) {
                SetCurrentNode(Current.GetExitNode());
            }
        }

        public void ChangeNode(RuntimeStateNode node) {
            SetCurrentNode(node);
        }

        protected virtual void SetCurrentNode(RuntimeStateNode node) {
            var last = Current;
            if (last != null) {
                last.OnExit();
            }
            Current = node;
            if (Current == null) {
                //OnComplete();
                return;
            }
            Current.OnEnter(last);
        }

        public void Trigger(string key) {
            if (_triggers.TryGetValue(key, out var trigger)) {
                trigger.Trigger();
            }
        }

        public void ResetTrigger(string key) {
            if (_triggers.TryGetValue(key, out var trigger)) {
                trigger.Reset();
            }
        }

        public bool IsTriggerActive(string key) {
            return _triggers.TryGetValue(key, out var trigger) && trigger.Triggered;
        }

        public RuntimeStateNode GetRuntimeNode(int id) {
            return _lookup.TryGetValue(id, out var node) ? node : null;
        }

        public RuntimeStateNode CreateRuntimeNode(StateGraphNode node) {
            if (_lookup.TryGetValue(node.Id, out var existing)) {
                return existing;
            }
            var runtimeNode = node.GetRuntimeNode(this);
            _lookup.Add(node.Id, runtimeNode);
            //_allNodes.Add(runtimeNode);
            return runtimeNode;
        }

    }
}
