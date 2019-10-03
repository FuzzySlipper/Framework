using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XNode;

namespace PixelComrades {

    public class RuntimeAnimationGraph : RuntimeStateGraph {
        
        public GraphAnimation Graph { get; }
        
        private Dictionary<string, AnimationNodeTrigger> _triggers = new Dictionary<string, AnimationNodeTrigger>();
        
        public RuntimeAnimationGraph(GraphAnimation graph) {
            Graph = graph;
            for (int i = 0; i < graph.nodes.Count; i++) {
                var node = graph.nodes[i] as ConvertibleNode;
                if (node == null) {
                    continue;
                }
                CreateRuntimeNode(node);
            }
            StartNode = GetRuntimeNode(graph.StartNode.Id);
            for (int i = 0; i < graph.Triggers.Count; i++) {
                _triggers.AddOrUpdate(graph.Triggers[i].Key, graph.Triggers[i]);
            }
        }

        public void Trigger(string key) {
            if (_triggers.TryGetValue(key, out var trigger)) {
                trigger.Trigger();
            }
        }

        public bool TriggerActive(string key) {
            return _triggers.TryGetValue(key, out var trigger) && trigger.Triggered;
        }
        
        public override void Start() {
            base.Start();
        }
//
//        public void Start(AnimationGraphEvent graphEvent) {
//            
//        }

    }
    public class GraphAnimation : NodeGraph {
        
        public List<AnimationNodeTrigger> Triggers = new List<AnimationNodeTrigger>();
        public ConvertibleNode StartNode { get; private set; }

        public RuntimeAnimationGraph GetRuntimeGraph() {
            if (StartNode == null) {
                for (int i = 0; i < nodes.Count; i++) {
                    var node = nodes[i] as ConvertibleNode;
                    if (node == null) {
                        continue;
                    }
                    bool hasConnection = false;
                    foreach (NodePort nodePort in node.Inputs) {
                        if (nodePort.IsConnected) {
                            hasConnection = true;
                            break;
                        }
                    }
                    if (!hasConnection) {
                        StartNode = node;
                        break;
                    }
                }
            }
            return new RuntimeAnimationGraph(this);
        }
    }
}
