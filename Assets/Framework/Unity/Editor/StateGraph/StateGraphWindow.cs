using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    public class StateGraphWindow : EditorWindow {

        private const int StyleIndexDefault = 3;
        private const int StyleBlockGlobal = 2;
        private const int StyleIndexGlobal = 1;
        
        private StateGraph _graph;
        private Vector2 _mousePosition;
        private Vector2 _drag;
        private Vector2 _offset;
        private GUIStyle _inPointStyle;
        private GUIStyle _outPointStyle;
        private GUIStyle _nodeTextStyle;
        private GUIStyle _nodeButtonStyle;
        private GUIStyle[] _nodeStyles;
        private GUIStyle[] _nodeSelectedStyles;
        
        private ConnectionPoint _selectedInPoint;
        private ConnectionPoint _selectedOutPoint;
        private List<Type> _nodeTypes = new List<Type>();
        private GenericMenu _addTrackMenu;
        private StateGraphNode _selected;
        private StateGraphNode _dragged;
        
        [MenuItem("Window/State Graph Window")]
        public static StateGraphWindow ShowWindow() {
            StateGraphWindow window = GetWindow<StateGraphWindow>();
            window.titleContent = new GUIContent("StateGraphWindow");
            return window;
        }

        public StateGraphWindow() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type.IsSubclassOf(typeof(StateGraphNode))) {
                        _nodeTypes.Add(type);
                    }
                }
            }
        }

        void OnEnable() {
            SetupStyles();
        }

        private void SetupStyles() {
            _nodeStyles = new GUIStyle[4];
            _nodeSelectedStyles = new GUIStyle[4];
            var path = "builtin skins/darkskin/images/node";
            for (int i = 0; i < _nodeStyles.Length; i++) {
                int index = i;
                _nodeStyles[i] = new GUIStyle();
                _nodeStyles[i].normal.background = EditorGUIUtility.Load(string.Format("{0}{1}.png", path, index)) as Texture2D;
                _nodeStyles[i].border = new RectOffset(12, 12, 12, 12);
                _nodeSelectedStyles[i] = new GUIStyle();
                _nodeSelectedStyles[i].normal.background = EditorGUIUtility.Load(string.Format("{0}{1} on.png", path, index)) as Texture2D;
                _nodeSelectedStyles[i].border = new RectOffset(12, 12, 12, 12);
            }
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
            _nodeButtonStyle = new GUIStyle("ToolbarButton");
        }

        public void Set(StateGraph graph) {
            _graph = graph;
        }

        private void OnGUI() {
            if (_inPointStyle == null || _nodeStyles == null || _nodeTextStyle == null) {
                SetupStyles();
            }
            if (_nodeTypes.Count == 0) {
                GUILayout.Label("No Sequence Types");
            }
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            DrawNodes();
            DrawConnections();
            DrawConnectionLine(Event.current);
            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);
            CheckNames();
            if (GUI.changed) {
                Repaint();
            }
        }

        private void DrawConnectionLine(Event e) {
            if (_selectedInPoint != null && _selectedOutPoint == null) {
                Handles.DrawBezier(
                    _selectedInPoint.Rect.center,
                    e.mousePosition,
                    _selectedInPoint.Rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );
                GUI.changed = true;
            }
            if (_selectedOutPoint != null && _selectedInPoint == null) {
                Handles.DrawBezier(
                    _selectedOutPoint.Rect.center,
                    e.mousePosition,
                    _selectedOutPoint.Rect.center - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );
                GUI.changed = true;
            }
        }

        private void DrawConnections() {
            for (int c = 0; c < _graph.Connections.Count; c++) {
                var connect = _graph.Connections[c];
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

                if (Handles.Button((connectIn.Rect.center + connectOut.Rect.center) * 0.5f, Quaternion.identity,
                    4, 8, Handles.RectangleHandleCap)) {
                    OnClickRemoveConnection(connect);
                }
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
            for (int i = 0; i < _graph.Nodes.Count; i++) {
                var node = _graph.Nodes[i];
                var maxWidth = node.Rect.x * 0.8f;
                int styleIndex = 0;
                if (node == _graph.Default) {
                    styleIndex = StyleIndexDefault;
                }
                else if (node.IsGlobal) {
                    styleIndex = StyleIndexGlobal;
                }
                else if (node.BlockAnyStateChecks) {
                    styleIndex = StyleBlockGlobal;
                }
                var style = node == _selected ? _nodeSelectedStyles[styleIndex] : _nodeStyles[styleIndex];
                GUILayout.BeginArea(node.Rect, style);
                GUILayout.Space(10);
                GUILayout.Label(node.Title, _nodeTextStyle, GUILayout.MaxWidth(maxWidth));
                GUILayout.Label("ID: " + node.Id, _nodeTextStyle, GUILayout.MaxWidth(maxWidth));
                EditorGUI.BeginChangeCheck();
                if (node.DrawGui(_nodeTextStyle, _nodeButtonStyle)) {
                    Repaint();
                }
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(node);
                }
                GUILayout.EndArea();
                for (int c = 0; c < node.InPoints.Count; c++) {
                    if (node.InPoints[c] == null) {
                        node.InPoints.RemoveAt(c);
                        break;
                    }
                    DrawConnectionPoint(node.InPoints[c], c);
                }
                for (int c = 0; c < node.OutPoints.Count; c++) {
                    if (node.OutPoints[c] == null) {
                        node.OutPoints.RemoveAt(c);
                        break;
                    }
                    DrawConnectionPoint(node.OutPoints[c], c);
                }
                if (node.InPoints.Count < node.InputMax) {
                    var addRect = new Rect(
                        node.Rect.x + ConnectionPoint.Width * 0.5f,
                        node.Rect.y + node.Rect.height - (ConnectionPoint.Height*1.25f),
                        ConnectionPoint.Width, ConnectionPoint.Height);
                    if (GUI.Button(addRect, "+", _nodeTextStyle)) {
                        node.InPoints.Add(new ConnectionPoint(node, ConnectionPointType.In, node.FindMinConnectionId()));
                        EditorUtility.SetDirty(node);
                    }
                }
                if (node.OutPoints.Count < node.OutputMax) {
                    var addRect = new Rect(
                        node.Rect.x + node.Rect.width - (ConnectionPoint.Width*1.25f),
                        node.Rect.y + node.Rect.height - (ConnectionPoint.Height*1.25f),
                        ConnectionPoint.Width, ConnectionPoint.Height);
                    if (GUI.Button(addRect, "+", _nodeTextStyle)) {
                        node.OutPoints.Add(new ConnectionPoint(node, ConnectionPointType.Out, node.FindMinConnectionId()));
                        EditorUtility.SetDirty(node);
                    }
                }
            }
        }

        private void DrawConnectionPoint(ConnectionPoint point, int index) {
            var height = (point.Node.Rect.height * ((index+1) * 0.2f)); 
            point.Rect.y = point.Node.Rect.y + height - point.Rect.height * 0.5f;
            switch (point.ConnectType) {
                case ConnectionPointType.In:
                    point.Rect.x = point.Node.Rect.x - point.Rect.width + 8f;
                    break;

                case ConnectionPointType.Out:
                    point.Rect.x = point.Node.Rect.x + point.Node.Rect.width - 8f;
                    break;
            }
            if (GUI.Button(point.Rect, "",point.ConnectType == ConnectionPointType.In ? _inPointStyle : _outPointStyle)) {
                if (point.ConnectType == ConnectionPointType.In) {
                    OnClickInPoint(point);
                }
                else {
                    OnClickOutPoint(point);
                }
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
            if (GUI.Button(buttonRect, "-", _nodeTextStyle)) {
                RemoveConnectionPoint(point);
            }
        }

        private void RemoveConnectionPoint(ConnectionPoint point) {
            for (int i = _graph.Connections.Count - 1; i >= 0; i--) {
                if (_graph.Connections[i].GetIn() == point || _graph.Connections[i].GetOut() == point) {
                    _graph.Connections.RemoveAt(i);
                }
            }
            if (point.ConnectType == ConnectionPointType.In) {
                point.Node.InPoints.Remove(point);
            }
            else {
                point.Node.OutPoints.Remove(point);
            }
            EditorUtility.SetDirty(point.Node);
        }
        
        private void ProcessNodeEvents(Event e) {
            if (_graph == null) {
                return;
            }
            for (int i = _graph.Nodes.Count - 1; i >= 0; i--) {
                var node = _graph.Nodes[i];
                bool guiChanged = false;
                switch (e.type) {
                    case EventType.MouseDown:
                        switch (e.button) {
                            case 0:
                                if (node.Rect.Contains(e.mousePosition)) {
                                    _selected = _dragged = node;
                                    GUI.changed = true;
                                }
                                else if (_selected == node) {
                                    _selected = null;
                                    GUI.changed = true;
                                }
                                break;
                            case 1:
                                if (node.Rect.Contains(e.mousePosition)) {
                                    _selected = node;
                                    GUI.changed = true;
                                    GenericMenu genericMenu = new GenericMenu();
                                    genericMenu.AddItem(new GUIContent("Remove node"), false, () => OnClickRemoveNode(node));
                                    genericMenu.AddItem(new GUIContent("Toggle Default"), false, () => OnClickSetDefault(node));
                                    genericMenu.AddItem(new GUIContent((node.BlockAnyStateChecks ? "Disable" : "Enable") + " Any State Checking"),
                                        false, () => OnClickToggleGlobalBlock
                                    (node));
                                    genericMenu.ShowAsContext();
                                    e.Use();
                                }
                                break;
                        }
                        break;
                    case EventType.MouseUp:
                        _dragged = null;
                        break;

                    case EventType.MouseDrag:
                        if (e.button == 0 && _dragged == node) {
                            node.Drag(e.delta);
                            e.Use();
                            guiChanged = true;
                            EditorUtility.SetDirty(node);
                        }
                        break;
                }
                if (guiChanged) {
                    GUI.changed = true;
                }
            }
        }

        private void ProcessEvents(Event e) {
            _drag = Vector2.zero;
            _mousePosition = e.mousePosition;
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0) {
                        ClearConnectionSelection();
                    }
                    if (e.button == 1) {
                        if (_addTrackMenu == null) {
                            _addTrackMenu = new GenericMenu();
                            for (var i = 0; i < _nodeTypes.Count; i++) {
                                _addTrackMenu.AddItem(new GUIContent(_nodeTypes[i].ToString()), false, CreateContextItem,
                                    _nodeTypes[i]);
                            }
                        }
                        _addTrackMenu.ShowAsContext();
                    }
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0) {
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
            _drag = delta;
            if (_graph != null) {
                for (int i = 0; i < _graph.Nodes.Count; i++) {
                    _graph.Nodes[i].Drag(delta);
                    EditorUtility.SetDirty(_graph.Nodes[i]);
                }
            }
            GUI.changed = true;
        }
        
        private void ClearConnectionSelection() {
            _selectedInPoint = null;
            _selectedOutPoint = null;
        }

        private void CreateConnection() {
            _graph.Connections.Add(new Connection(_selectedInPoint, _selectedOutPoint));
        }

        private void OnClickInPoint(ConnectionPoint inPoint) {
            _selectedInPoint = inPoint;
            if (_selectedOutPoint != null) {
                if (_selectedOutPoint.Node != _selectedInPoint.Node) {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(ConnectionPoint outPoint) {
            _selectedOutPoint = outPoint;
            if (_selectedInPoint != null) {
                if (_selectedOutPoint.Node != _selectedInPoint.Node) {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveConnection(Connection connection) {
            _graph.Connections.Remove(connection);
        }

        private void OnClickRemoveNode(StateGraphNode node) {
            if (_graph.Default == node) {
                _graph.Default = null;
            }
            _graph.ClearConnectsWith(node);
            _graph.Nodes.Remove(node);
            DestroyImmediate(node, true);
            EditorUtility.SetDirty(_graph);
            Repaint();
        }

        private void OnClickSetDefault(StateGraphNode node) {
            if (_graph.Default == node) {
                _graph.Default = null;
            }
            else {
                _graph.Default = node;
            }
            EditorUtility.SetDirty(_graph);
            Repaint();
        }

        private void OnClickToggleGlobalBlock(StateGraphNode node) {
            node.BlockAnyStateChecks = !node.BlockAnyStateChecks;
            EditorUtility.SetDirty(node);
            Repaint();
        }

        private void CreateContextItem(object obj) {
            var targetType = obj as Type;
            if (targetType == null) {
                return;
            }
            var newObj = CreateInstance(targetType);
            AssetDatabase.AddObjectToAsset(newObj, _graph);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
            var node = (StateGraphNode) newObj;
            _graph.Nodes.Add(node);
            node.Set(_mousePosition, GetUniqueId(), _graph);
            Repaint();
        }

        

        private int GetUniqueId() {
            int id = 1000;
            for (int i = 0; i < _graph.Nodes.Count; i++) {
                id = Mathf.Max(id, _graph.Nodes[i].Id);
            }
            id++;
            return id;
        }

        private void CheckNames() {
            for (int i = 0; i < _graph.Nodes.Count; i++) {
                var properName = string.Format("{0}:{1}", _graph.Nodes[i].Id, _graph.Nodes[i].Title);
                if (_graph.Nodes[i].name != properName) {
                    _graph.Nodes[i].name = properName;
                    EditorUtility.SetDirty(_graph.Nodes[i]);
                }
            }
        }
    }
}