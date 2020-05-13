using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace PixelComrades {
    public class StateGraphDebugWindow : EditorWindow {

        private const float Border = 300;
        private const float MaxRectSize = 2500;

        private Vector2 _scrollPosition;
        private Rect _scrollRect;
        private Vector2 _scrollPosition1;
        private AnimationGraphComponent _graph;
        
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

        private void Update() {
            if (!Application.isPlaying) {
                Close();
                return;
            }
            // This is necessary to make the framerate normal for the editor window.
            Repaint();
        }
        
        private void OnGUI() {
            if (!Application.isPlaying) {
                Close();
                return;
            }
            if (_inPointStyle == null || _nodeStyle == null || _nodeTextStyle == null) {
                SetupStyles();
            }
            var list = EntityController.GetComponentArray<AnimationGraphComponent>();
            _entityListing.Clear();
            _smallerList.Clear();
            int currentIndex = -1;
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
            _scrollRect = new Rect(0, 0, position.width - Border, position.height);
            _scrollPosition = GUI.BeginScrollView(_scrollRect, _scrollPosition, new Rect(0, 0, MaxRectSize, MaxRectSize));
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            if (_graph == null) {
                EditorGUILayout.LabelField("No Graph Selected of " + _entityListing.Count);
                GUI.EndScrollView();
                return;
            }
            if (_graph.Value == null) {
                EditorGUILayout.LabelField("Component has no Graph " + _graph.GetEntity()?.DebugId ?? "None");
                GUI.EndScrollView();
                return;
            }
            if (_graph.Value.Current is ExternalGraphNode.RuntimeNode externalGraphNode) {
                DrawGraph(externalGraphNode.ExternalGraph);
            }
            else if (_graph.Value.Current is SwitchExternalNode.RuntimeNode switchGraphNode) {
                DrawGraph(switchGraphNode.ExternalGraph);
            }
            else {
                DrawGraph(_graph.Value);
            }
        }

        private void DrawGraph(RuntimeStateGraph graph) {
            DrawConnections(graph);
            DrawNodes(graph);
            GUI.EndScrollView();

            ProcessEvents(Event.current);
            var sideRect = new Rect(position.width - Border, 0, Border - 10, position.height);
            _scrollPosition1 = GUI.BeginScrollView(sideRect, _scrollPosition1, new Rect(0, 0, Border, MaxRectSize));
            EditorGUILayout.LabelField("IsActive " + graph.IsActive);
            EditorGUILayout.LabelField("Current " + graph.Current != null ? graph.Current.Node.name : "null");
            EditorGUILayout.LabelField(" ");
            EditorGUILayout.LabelField("-Variables-");
            foreach (var valueVariable in graph.Variables) {
                EditorGUILayout.LabelField(valueVariable.Key + ": ");
                EditorGUILayout.LabelField(valueVariable.Value.ToString());
            }
            EditorGUILayout.LabelField(" ");
            EditorGUILayout.LabelField("-Triggers-");
            foreach (string trigger in graph.TriggerLog.InOrder()) {
                EditorGUILayout.LabelField(graph.TriggerLog.GetTime(trigger).ToString("F3") + ": ");
                EditorGUILayout.LabelField(trigger);
            }
            GUI.EndScrollView();
            Repaint();
        }

        private void DrawConnections(RuntimeStateGraph graph) {
            for (int i = 0; i < graph.OriginalGraph.Count; i++) {
                var node = graph.OriginalGraph[i];
                for (int c = 0; c < node.OutPoints.Count; c++) {
                    var connectOut = node.OutPoints[c];
                    if (connectOut.Target == null) {
                        continue;
                    }
                    var connectIn = connectOut.Target.GetConnectionInPointById(connectOut.TargetId);
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
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
            int widthDivs = Mathf.CeilToInt(MaxRectSize / gridSpacing);
            int heightDivs = Mathf.CeilToInt(MaxRectSize / gridSpacing);
            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            //_offset += _drag * 0.5f;
            //Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);
            for (int i = 0; i < widthDivs; i++) {
                Handles.DrawLine(
                    new Vector3(gridSpacing * i, -gridSpacing, 0),
                    new Vector3(gridSpacing * i, MaxRectSize, 0f));
            }
            for (int j = 0; j < heightDivs; j++) {
                Handles.DrawLine(
                    new Vector3(-gridSpacing, gridSpacing * j, 0),
                    new Vector3(MaxRectSize, gridSpacing * j, 0f));
            }
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes(RuntimeStateGraph graph) {
            if (graph == null) {
                return;
            }
            var currentNode = graph.Current?.Node;
            for (int i = 0; i < graph.OriginalGraph.Count; i++) {
                var node = graph.OriginalGraph.Nodes[i];
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
                GUILayout.Label(graph.GetRuntimeNode(node.Id).DebugInfo);
                GUILayout.EndArea();
                var inSpacing = Mathf.Clamp(0.8f / node.InPoints.Count, 0.02f, 1);
                for (int c = 0; c < node.InPoints.Count; c++) {
                    if (node.InPoints[c] == null) {
                        node.InPoints.RemoveAt(c);
                        break;
                    }
                    StateGraphExtensions.DrawConnectionPoint(node.InPoints[c], c, inSpacing, null, null);
                }
                var outSpacing = Mathf.Clamp(0.8f / node.OutPoints.Count, 0.02f, 1);
                for (int c = 0; c < node.OutPoints.Count; c++) {
                    if (node.OutPoints[c] == null) {
                        node.OutPoints.RemoveAt(c);
                        break;
                    }
                    StateGraphExtensions.DrawConnectionPoint(node.OutPoints[c], c, outSpacing, null, null);
                }
            }
        }

        private void ProcessEvents(Event e) {
            switch (e.type) {
                case EventType.MouseDrag:
                    if (e.button == 0 && e.clickCount < 1) {
                        OnDrag(e.delta);
                    }
                    break;
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
            if (_scrollRect.Contains(Event.current.mousePosition)) {
                _scrollPosition += -delta;
                Event.current.Use();
            }
        }
    }
}