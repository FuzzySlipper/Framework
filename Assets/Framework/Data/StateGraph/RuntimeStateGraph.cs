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
        public Dictionary<string, System.Object> Variables { get; }
        public float TimeStartGraph { get; protected set; }
        public RuntimeStateNode Current { get; protected set; }
        public RuntimeStateNode StartNode { get; protected set; }

        public bool IsActive { get { return Current != null; } }
        public string CurrentTag { get { return Current != null ? Current.Node.Tag : ""; } }
        public StateGraph OriginalGraph { get; private set; }
        public RuntimeStateGraph ParentGraph { get; private set; }
        public Entity Entity { get; private set; }
#if UNITY_EDITOR
        public CircularBuffer<string> TriggerLog = new CircularBuffer<string>(20, true);
#endif

        public RuntimeStateGraph(StateGraph graph, Entity entity) {
            GlobalTriggers = new Dictionary<string, GraphTrigger>();
            Variables =  new Dictionary<string, System.Object>();
            OriginalGraph = graph;
            ParentGraph = null;
            Entity = entity;
            SetupGraph();
        }

        public RuntimeStateGraph(RuntimeStateGraph parent, StateGraph graph, Entity entity) {
            GlobalTriggers = parent.GlobalTriggers;
            Variables = parent.Variables;
            OriginalGraph = graph;
            ParentGraph = parent;
            Entity = entity;
            SetupGraph();
        }

        public void Dispose() {
            foreach (var stateNode in _lookup) {
                stateNode.Value.Dispose();
            }
            _lookup.Clear();
            Entity = null;
            ParentGraph = null;
            OriginalGraph = null;
            Current = null;
            StartNode = null;
        }
        
        private void SetupGraph() {
            for (int i = 0; i < OriginalGraph.GlobalTriggers.Count; i++) {
                GlobalTriggers.AddOrUpdate(OriginalGraph.GlobalTriggers[i].Key, new GraphTrigger(OriginalGraph.GlobalTriggers[i]));
            }
            for (int i = 0; i < OriginalGraph.Count; i++) {
                CreateRuntimeNode(OriginalGraph[i]);
            }
            StartNode = GetRuntimeNode(OriginalGraph.Default != null ? OriginalGraph.Default.Id : OriginalGraph[0].Id);
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
            if (!Current.TryComplete(dt)) {
                return;
            }
            var node = Current.GetExitNode();
            if (node != null) {
                SetCurrentNode(node);
            }
            else {
#if UNITY_EDITOR
                TriggerLog.Add(string.Format("{0} had error trying to exit {1}", OriginalGraph.name, Current.Node.name));
#endif
                SetCurrentNode(null);
            }
        }

        public void ChangeNode(int id) {
            ChangeNode(GetRuntimeNode(id));
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

        public bool TriggerGlobal(string key) {
            if (!GlobalTriggers.TryGetValue(key, out var trigger)) {
                return false;
            }
            if (!trigger.Trigger()) {
                return false;
            }
            for (int i = 0; i < _globals.Count; i++) {
                _globals[i].CheckConditions();
            }
            trigger.Reset();
#if UNITY_EDITOR
            TriggerLog.Add(key);
#endif
            return true;
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
