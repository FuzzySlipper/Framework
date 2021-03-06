using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public abstract class StateGraphNode : ScriptableObject {
        
        protected const int MaxConnections = 5;
        protected const int MinConnectionId = 100;
        
        public static Vector2 DefaultNodeSize = new Vector2(225, 125);
        
        public Rect Rect;
        public List<ConnectionInPoint> InPoints = new List<ConnectionInPoint>();
        public List<ConnectionOutPoint> OutPoints = new List<ConnectionOutPoint>();
        public List<ConditionExit> Conditions = new List<ConditionExit>();
        public int DefaultExit = 0;
        public int Id;
        public string Tag;
        public StateGraph Graph;
        public bool AllowEarlyExit = false;
        public string EnterEvent = AnimationEvents.None;
        public string ExitEvent = AnimationEvents.None;
        
        protected virtual Vector2 GetNodeSize { get { return DefaultNodeSize; } }
        public virtual bool HasConditions { get { return Conditions.Count > 0; } }

        public void Set(Vector2 position, int id, StateGraph graph) {
            Rect = new Rect(position.x, position.y, GetNodeSize.x, GetNodeSize.y);
            Id = id;
            Graph = graph;
            int connectionId = MinConnectionId;
            if (InPoints.Count < InputMin) {
                for (int i = 0; i < InputMin; i++) {
                    InPoints.Add(new ConnectionInPoint(this, connectionId));
                    connectionId++;
                }
            }
            if (OutPoints.Count < OutputMin) {
                for (int i = 0; i < OutputMin; i++) {
                    OutPoints.Add(new ConnectionOutPoint(this, connectionId));
                    connectionId++;
                }
            }
        }
        
        public void Drag(Vector2 delta) {
            Rect.position += delta;
        }

        public ConnectionInPoint GetConnectionInPointById(int id) {
            for (int i = 0; i < InPoints.Count; i++) {
                if (InPoints[i].Id == id) {
                    return InPoints[i];
                }
            }
            return null;
        }

        public ConnectionOutPoint GetConnectionOutPointById(int id) {
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

        public void Remove(ConditionExit config) {
            Conditions.Remove(config);
            CheckSize();
        }

        public void CheckSize() {
            Rect.size = new Vector2(GetNodeSize.x, GetNodeSize.y + ((DefaultNodeSize.y * 0.5f) * Conditions.Count));
        }
        
        public abstract RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph);
        public abstract bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle);
        public abstract string Title { get; }
        public virtual int InputMin { get => 1; }
        public virtual int InputMax { get => MaxConnections; }
        public virtual int OutputMin { get => 1; }
        public virtual int OutputMax { get => MaxConnections; }
        public virtual bool IsGlobal { get { return false; } }
        public virtual int MaxConditions { get => 4; }

    }
//
//    [System.Serializable]
//    public class Connection {
//        public int In;
//        public StateGraphNode InNode;
//        public int Out;
//        public StateGraphNode OutNode;
//
//        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint) {
//            In = inPoint.Id;
//            InNode = inPoint.Owner;
//            Out = outPoint.Id;
//            OutNode = outPoint.Owner;
//        }
//
//        public ConnectionPoint GetIn() {
//            return InNode.GetConnectionPointById(In);
//        }
//
//        public ConnectionPoint GetOut() {
//            return OutNode.GetConnectionPointById(Out);
//        }
//    }

    [System.Serializable]
    public class ConnectionOutPoint {
        public const float Width = 15;
        public const float Height = 20;
        public Rect Rect;
        public StateGraphNode Owner;
        public StateGraphNode Target;
        public int Id;
        public int TargetId;

        public ConnectionOutPoint(StateGraphNode node, int id) {
            Owner = node;
            Rect = new Rect(0, 0, Width, Height);
            Id = id;
        }
    }

    [System.Serializable]
    public class ConnectionInPoint {
        public const float Width = 15;
        public const float Height = 20;
        public Rect Rect;
        public StateGraphNode Owner;
        public int Id;

        public ConnectionInPoint(StateGraphNode node, int id) {
            Owner = node;
            Rect = new Rect(0, 0, Width, Height);
            Id = id;
        }
    }
}
