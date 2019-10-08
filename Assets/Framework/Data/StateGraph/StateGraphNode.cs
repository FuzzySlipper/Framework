using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class StateGraphNode : ScriptableObject {
        
        protected const int MaxConnections = 4;
        protected const int MinConnectionId = 100;
        
        protected static Vector2 DefaultNodeSize = new Vector2(125, 100);
        
        public Rect Rect;
        public List<ConnectionPoint> InPoints = new List<ConnectionPoint>();
        public List<ConnectionPoint> OutPoints = new List<ConnectionPoint>();
        public int Id;
        public StateGraph Graph;
        
        protected virtual Vector2 GetNodeSize { get { return DefaultNodeSize; } }

        public void Set(Vector2 position, int id, StateGraph graph) {
            Rect = new Rect(position.x, position.y, GetNodeSize.x, GetNodeSize.y);
            Id = id;
            Graph = graph;
            int connectionId = MinConnectionId;
            for (int i = 0; i < InputMin; i++) {
                InPoints.Add(new ConnectionPoint(this, ConnectionPointType.In, connectionId));
                connectionId++;
            }
            for (int i = 0; i < OutputMin; i++) {
                OutPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, connectionId));
                connectionId++;
            }
        }
        
        public void Drag(Vector2 delta) {
            Rect.position += delta;
        }

        public ConnectionPoint GetConnectionPoint(int id) {
            for (int i = 0; i < InPoints.Count; i++) {
                if (InPoints[i].Id == id) {
                    return InPoints[i];
                }
            }
            for (int i = 0; i < OutPoints.Count; i++) {
                if (OutPoints[i].Id == id) {
                    return OutPoints[i];
                }
            }
            return null;
        }

        public int FindMinConnectionId() {
            int id = MinConnectionId;
            for (int i = 0; i < InPoints.Count; i++) {
                if (InPoints[i] == null) {
                    continue;
                }
                id = Mathf.Max(id, InPoints[i].Id);
            }
            for (int i = 0; i < OutPoints.Count; i++) {
                if (OutPoints[i] == null) {
                    continue;
                }
                id = Mathf.Max(id, OutPoints[i].Id);
            }
            id++;
            return id;
        }
        
        public abstract RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph);
        public abstract bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle);
        public abstract string Title { get; }
        public virtual int InputMin { get => 1; }
        public virtual int InputMax { get => MaxConnections; }
        public virtual int OutputMin { get => 1; }
        public virtual int OutputMax { get => 1; }
        public virtual bool IsGlobal { get { return false; } }

    }

    [System.Serializable]
    public class Connection {
        public int In;
        public StateGraphNode InNode;
        public int Out;
        public StateGraphNode OutNode;

        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint) {
            In = inPoint.Id;
            InNode = inPoint.Node;
            Out = outPoint.Id;
            OutNode = outPoint.Node;
        }

        public ConnectionPoint GetIn() {
            return InNode.GetConnectionPoint(In);
        }

        public ConnectionPoint GetOut() {
            return OutNode.GetConnectionPoint(Out);
        }
    }

    public enum ConnectionPointType {
        In,
        Out
    }

    [System.Serializable]
    public class ConnectionPoint {
        public const float Width = 15;
        public const float Height = 20;
        public Rect Rect;
        public ConnectionPointType ConnectType;
        public StateGraphNode Node;
        public int Id;

        public ConnectionPoint(StateGraphNode node, ConnectionPointType connectType, int id) {
            Node = node;
            ConnectType = connectType;
            Rect = new Rect(0, 0, Width, Height);
            Id = id;
        }
    }
}
