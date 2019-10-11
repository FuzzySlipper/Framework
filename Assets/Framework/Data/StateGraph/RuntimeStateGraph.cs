using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = System.Object;

namespace PixelComrades {

    public class RuntimeStateGraph {
    
        public System.Action OnComplete;
        
        private Dictionary<int, RuntimeStateNode> _lookup = new Dictionary<int, RuntimeStateNode>();
        private List<IGlobalRuntimeStateNode> _globals = new List<IGlobalRuntimeStateNode>();
        
        private Dictionary<string, GraphTrigger> GlobalTriggers { get; }
        private Dictionary<string, System.Object> Variables { get; }
        public float TimeStartGraph { get; protected set; }
        public RuntimeStateNode Current { get; protected set; }
        public RuntimeStateNode StartNode { get; protected set; }

        public bool IsActive { get { return Current != null; } }
        public StateGraph OriginalGraph { get; private set; }
        public RuntimeStateGraph ParentGraph { get; }
        public Entity Entity { get; private set; }

        public RuntimeStateGraph(StateGraph graph) {
            GlobalTriggers = new Dictionary<string, GraphTrigger>();
            Variables =  new Dictionary<string, System.Object>();
            OriginalGraph = graph;
            ParentGraph = null;
            SetupGraph();
        }

        public RuntimeStateGraph(RuntimeStateGraph parent, StateGraph graph) {
            GlobalTriggers = parent.GlobalTriggers;
            Variables = parent.Variables;
            OriginalGraph = graph;
            ParentGraph = parent;
            SetupGraph();
        }

        private void SetupGraph() {
            for (int i = 0; i < OriginalGraph.GlobalTriggers.Count; i++) {
                GlobalTriggers.AddOrUpdate(OriginalGraph.GlobalTriggers[i].Key, OriginalGraph.GlobalTriggers[i]);
            }
            for (int i = 0; i < OriginalGraph.Count; i++) {
                CreateRuntimeNode(OriginalGraph[i]);
            }
            StartNode = GetRuntimeNode(OriginalGraph.Default != null ? OriginalGraph.Default.Id : OriginalGraph[0].Id);
        }
        
        public void SetOwner(Entity owner) {
            Entity = owner;
        }

        public T GetVariable<T>(string key) {
            if (Variables.TryGetValue(key, out var variable) && variable is T targetType) {
                return targetType;
            }
            return default(T);
        }

        public void SetVariable<T>(string key, T value) {
            if (Variables.ContainsKey(key)) {
                Variables[key] = value;
            }
            else {
                Variables.Add(key, value);
            }
        }

        public virtual void Start() {
            TimeStartGraph = TimeManager.Time;
            SetCurrentNode(StartNode);
        }

        public void Stop() {
            if (Current != null) {
                Current.OnExit();
            }
            Current = null;
        }

        public virtual void Update(float dt) {
            if (Current == null) {
                return;
            }
            for (int i = 0; i < _globals.Count; i++) {
                _globals[i].CheckConditions();
            }
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
                GraphCompleted();
                return;
            }
            Current.OnEnter(last);
        }

        public void GraphCompleted() {
            OnComplete.SafeInvoke();
        }

        public void TriggerGlobal(string key) {
            if (!GlobalTriggers.TryGetValue(key, out var trigger)) {
                return;
            }
            if (!trigger.Trigger()) {
                return;
            }
            for (int i = 0; i < _globals.Count; i++) {
                _globals[i].CheckConditions();
            }
            trigger.Reset();
        }

        public bool IsGlobalTriggerActive(string key) {
            return GlobalTriggers.TryGetValue(key, out var trigger) && trigger.Triggered;
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
            if (runtimeNode is IGlobalRuntimeStateNode global) {
                _globals.Add(global);
            }
            //_allNodes.Add(runtimeNode);
            return runtimeNode;
        }

    }
}
