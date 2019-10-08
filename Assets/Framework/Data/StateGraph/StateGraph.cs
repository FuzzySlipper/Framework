using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [CreateAssetMenu]
    public class StateGraph : ScriptableObject {
        public List<StateGraphNode> Nodes = new List<StateGraphNode>();
        public List<Connection> Connections = new List<Connection>();
        public List<GraphTrigger> Triggers = new List<GraphTrigger>();
        public StateGraphNode Default;
        
        public StateGraphNode this[int index] { get { return Nodes[index]; } }
        public int Count { get { return Nodes.Count; } }

        public void ClearConnectsWith(StateGraphNode node) {
            for (int i = Connections.Count - 1; i >= 0; i--) {
                if (Connections[i].GetIn().Node == node || Connections[i].GetOut().Node == node) {
                    Connections.RemoveAt(i);
                }
            }
        }

        public StateGraphNode GetConnectionEndpoint(ConnectionPoint point) {
            for (int i = 0; i < Connections.Count; i++) {
                if (Connections[i].GetIn() == point) {
                    return Connections[i].OutNode;
                }
            }
            return null;
        }

        public void FillConnectionList(StateGraphNode node, List<Connection> connectList) {
            for (int i = 0; i < Connections.Count; i++) {
                if (Connections[i].InNode == node || Connections[i].OutNode == node) {
                    connectList.Add(Connections[i]);
                }
            }
        }

        public RuntimeStateGraph GetRuntimeGraph(Entity entity) {
            var runtime = new RuntimeStateGraph(this);
            runtime.SetOwner(entity);
            return runtime;
        }
        
    }
}
