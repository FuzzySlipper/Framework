using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    public class StateGraphDebugWindow : EditorWindow {

        private const float MaxDrag = 50f;

        private AnimationGraphComponent _graph;
        private Vector2 _mousePosition;
        private Vector2 _drag;
        private Vector2 _offset;
        private GUIStyle _inPointStyle;
        private GUIStyle _outPointStyle;
        private GUIStyle _nodeTextStyle;
        private GUIStyle _nodeStyle;
        private GUIStyle _nodeSelectedStyle;

        [MenuItem("Window/State Graph Debugger")]
        public static StateGraphDebugWindow ShowWindow() {
            var window = GetWindow<StateGraphDebugWindow>();
            window.titleContent = new GUIContent("StateGraphWindow");
            return window;
        }

        void OnEnable() {
            SetupStyles();
        }

        private void SetupStyles() {
            var path = "builtin skins/darkskin/images/node";
            _nodeStyle = new GUIStyle();
            _nodeStyle.normal.background = EditorGUIUtility.Load(string.Format("{0}{1}.png", path, 0)) as Texture2D;
            _nodeStyle.border = new RectOffset(12, 12, 12, 12);
            _nodeSelectedStyle = new GUIStyle();
            _nodeSelectedStyle.normal.background = EditorGUIUtility.Load(string.Format("{0}{1} on.png", path, 1)) as Texture2D;
            _nodeSelectedStyle.border = new RectOffset(12, 12, 12, 12);
            _inPointStyle = new GUIStyle();
            _inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            _inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            _inPointStyle.border = new RectOffset(4, 4, 12, 12);
            _outPointStyle = new GUIStyle();
            _outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            _outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            _outPointStyle.border = new RectOffset(4, 4, 12, 12);
            _nodeTextStyle = new GUIStyle("WhiteLabel");
            _nodeTextStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        private List<string> _entityListing = new List<string>();
        private List<AnimationGraphComponent> _smallerList = new List<AnimationGraphComponent>();

        private void OnGUI() {
            if (_inPointStyle == null || _nodeStyle == null || _nodeTextStyle == null) {
                SetupStyles();
            }
            var list = EntityController.GetComponentArray<AnimationGraphComponent>();
            _entityListing.Clear();
            _smallerList.Clear();
            int currentIndex = 0;
            foreach (var graphComponent in list) {
                if (graphComponent == _graph) {
                    currentIndex = _entityListing.Count;
                }
                _entityListing.Add(graphComponent.GetEntity().DebugId);
                _smallerList.Add(graphComponent);
            }
            var newIndex = EditorGUILayout.Popup(currentIndex, _entityListing.ToArray());
            if (newIndex != currentIndex) {
                _graph = _smallerList[newIndex];
            }
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            if (_graph == null) {
                EditorGUILayout.LabelField("No Graph Selected of " + _entityListing.Count);
                return;
            }
            if (_graph.Value == null) {
                EditorGUILayout.LabelField("Component has no Graph " + _graph.GetEntity()?.DebugId ?? "None");
                return;
            }
            foreach (var valueVariable in _graph.Value.Variables) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(valueVariable.Key + ": ");
                EditorGUILayout.LabelField(valueVariable.Value.ToString());
                EditorGUILayout.EndHorizontal();
            }
            DrawConnections();
            DrawNodes();
            ProcessEvents(Event.current);
            if (GUI.changed) {
                Repaint();
            }
        }

        private void DrawConnections() {
            for (int c = 0; c < _graph.Value.OriginalGraph.Connections.Count; c++) {
                var connect = _graph.Value.OriginalGraph.Connections[c];
                var connectIn = connect.GetIn();
                var connectOut = connect.GetOut();
                Handles.DrawBezier(
                    connectIn.Rect.center,
                    connectOut.Rect.center,
                    connectIn.Rect.center + Vector2.left * 50f,
                    connectOut.Rect.center - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );
            }
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            _offset += _drag * 0.5f;
            Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);
            for (int i = 0; i < widthDivs; i++) {
                Handles.DrawLine(
                    new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                    new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }
            for (int j = 0; j < heightDivs; j++) {
                Handles.DrawLine(
                    new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                    new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes() {
            if (_graph == null) {
                return;
            }
            var currentNode = _graph.Value.Current.Node;
            for (int i = 0; i < _graph.Value.OriginalGraph.Count; i++) {
                var node = _graph.Value.OriginalGraph.Nodes[i];
                var maxWidth = node.Rect.x * 0.8f;
                var style = node == currentNode ? _nodeSelectedStyle : _nodeStyle;
                GUILayout.BeginArea(node.Rect, style);
                GUILayout.Space(10);
                GUILayout.Label(node.Title, _nodeTextStyle, GUILayout.MaxWidth(maxWidth));
                GUILayout.Label("ID: " + node.Id, _nodeTextStyle, GUILayout.MaxWidth(maxWidth));
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Enter");
                EditorGUILayout.LabelField(node.EnterEvent);
                GUILayout.Label("Exit");
                EditorGUILayout.LabelField(node.ExitEvent);
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
                var inSpacing = Mathf.Clamp(0.8f / node.InPoints.Count, 0.02f, 1);
                for (int c = 0; c < node.InPoints.Count; c++) {
                    if (node.InPoints[c] == null) {
                        node.InPoints.RemoveAt(c);
                        break;
                    }
                    DrawConnectionPoint(node.InPoints[c], c, inSpacing);
                }
                var outSpacing = Mathf.Clamp(0.8f / node.OutPoints.Count, 0.02f, 1);
                for (int c = 0; c < node.OutPoints.Count; c++) {
                    if (node.OutPoints[c] == null) {
                        node.OutPoints.RemoveAt(c);
                        break;
                    }
                    DrawConnectionPoint(node.OutPoints[c], c, outSpacing);
                }
            }
        }

        private void DrawConnectionPoint(ConnectionPoint point, int index, float spacing) {
            var height = (point.Node.Rect.height * ((index + 1) * spacing));
            point.Rect.y = point.Node.Rect.y + height - point.Rect.height * 0.5f;
            switch (point.ConnectType) {
                case ConnectionPointType.In:
                    point.Rect.x = point.Node.Rect.x - point.Rect.width + 8f;
                    break;

                case ConnectionPointType.Out:
                    point.Rect.x = point.Node.Rect.x + point.Node.Rect.width - 8f;
                    break;
            }
            var labelRect = new Rect(point.Rect);
            GUI.Label(labelRect, " " + index.ToString(), _nodeTextStyle);
            var buttonRect = new Rect(point.Rect);
            switch (point.ConnectType) {
                case ConnectionPointType.In:
                    if (point.Node.InPoints.Count <= point.Node.InputMin) {
                        return;
                    }
                    buttonRect.x += point.Rect.width + 1;
                    break;

                case ConnectionPointType.Out:
                    if (point.Node.OutPoints.Count <= point.Node.OutputMin) {
                        return;
                    }
                    buttonRect.x -= point.Rect.width + 1;
                    break;
            }
        }

        private void ProcessEvents(Event e) {
            _drag = Vector2.zero;
            _mousePosition = e.mousePosition;
            switch (e.type) {
//                case EventType.MouseDrag:
//                    if (e.button == 0 && e.clickCount < 1) {
//                        OnDrag(e.delta);
//                    }
//                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Escape) {
                        Close();
                    }
                    break;
            }
        }

        private void OnDrag(Vector2 delta) {
            if (EditorWindow.mouseOverWindow != this) {
                GUILayout.Label("Not Active Window");
                return;
            }
            _drag = Vector2.ClampMagnitude(delta, MaxDrag);
            if (_graph != null) {
                for (int i = 0; i < _graph.Value.OriginalGraph.Nodes.Count; i++) {
                    _graph.Value.OriginalGraph.Nodes[i].Drag(_drag);
                    EditorUtility.SetDirty(_graph.Value.OriginalGraph.Nodes[i]);
                }
            }
            GUI.changed = true;
        }
    }
}