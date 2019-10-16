using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [CreateAssetMenu]
    public class StateGraph : ScriptableObject {
        public List<StateGraphNode> Nodes = new List<StateGraphNode>();
        // public List<Connection> Connections = new List<Connection>();
        public List<GraphTrigger> GlobalTriggers = new List<GraphTrigger>();
        public StateGraphNode Default;
        
        public StateGraphNode this[int index] { get { return Nodes[index]; } }
        public int Count { get { return Nodes.Count; } }

        public void ClearConnectsWith(StateGraphNode node) {
            for (int i = 0; i < Nodes.Count; i++) {
                ClearList(Nodes[i].InPoints, node);
                ClearList(Nodes[i].OutPoints, node);
            }
//            for (int i = Connections.Count - 1; i >= 0; i--) {
//                if (Connections[i].GetIn().Owner == node || Connections[i].GetOut().Owner == node) {
//                    Connections.RemoveAt(i);
//                }
//            }
        }

        private void ClearList(List<ConnectionPoint> points, StateGraphNode node) {
            for (int c = 0; c < points.Count; c++) {
                if (points[c].Target == node) {
                    points[c].Target = null;
                    points[c].TargetId = -1;
                }
            }
        }

        public void ClearConnectsWith(ConnectionPoint newInPnt, ConnectionPoint newOutPnt) {
//            for (int i = Connections.Count - 1; i >= 0; i--) {
//                var inPnt = Connections[i].GetIn();
//                var outPnt = Connections[i].GetOut();
//                if (inPnt == newInPnt || inPnt == newOutPnt || outPnt == newInPnt || outPnt == newOutPnt) {
//                    Connections.RemoveAt(i);
//                }
//            }
            ClearConnectsWith(newInPnt);
            ClearConnectsWith(newOutPnt);
        }

        public void ClearConnectsWith(ConnectionPoint point) {
            for (int i = 0; i < Nodes.Count; i++) {
                ClearList(Nodes[i].InPoints, point);
                ClearList(Nodes[i].OutPoints, point);
            }
        }

        private void ClearList(List<ConnectionPoint> points, ConnectionPoint pnt) {
            for (int c = 0; c < points.Count; c++) {
                if (points[c].TargetId == pnt.Id && points[c].Target == pnt.Owner) {
                    points[c].Target = null;
                    points[c].TargetId = -1;
                }
            }
        }
//
//        public StateGraphNode GetConnectionEndpoint(ConnectionPoint point) {
//            for (int i = 0; i < Connections.Count; i++) {
//                if (Connections[i].GetIn().Id == point.Id) {
//                    return Connections[i].OutNode;
//                }
//            }
//            return null;
//        }
//
//        public void FillConnectionList(StateGraphNode node, List<Connection> connectList) {
//            for (int i = 0; i < Connections.Count; i++) {
//                if (Connections[i].InNode == node || Connections[i].OutNode == node) {
//                    connectList.Add(Connections[i]);
//                }
//            }
//        }

        public RuntimeStateGraph GetRuntimeGraph(Entity entity) {
            var runtime = new RuntimeStateGraph(this, entity);
            return runtime;
        }

        public RuntimeStateGraph GetRuntimeGraph(RuntimeStateGraph parent, Entity entity) {
            var runtime = new RuntimeStateGraph(parent, this, entity);
            return runtime;
        }
        
    }
}
