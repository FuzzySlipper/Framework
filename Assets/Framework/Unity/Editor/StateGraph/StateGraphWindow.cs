using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    public class StateGraphWindow : EditorWindow {

        private const int StyleArraySize = 5;
        private const int StyleHasEarlyExit = 4;
        private const int StyleHasConditions = 3;
        private const int StyleIndexDefault = 2;
        private const int StyleIndexGlobal = 1;
        private const float Border = 150;
        private const float MaxRectSize = 2500;

        private StateGraph _graph = null;
        private Vector2 _mouseScrollPosition;
        private Vector2 _scrollPosition;
        private Rect _scrollRect;
        private Vector2 _scrollPosition1;
        
        
        
        private GUIStyle _nodeButtonStyle;
        private GUIStyle[] _nodeStyles;
        private GUIStyle[] _nodeSelectedStyles;
        
        private ConnectionInPoint _selectedInPoint;
        private ConnectionOutPoint _selectedOutPoint;
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
            _nodeStyles = new GUIStyle[StyleArraySize];
            _nodeSelectedStyles = new GUIStyle[StyleArraySize];
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
            
            _nodeButtonStyle = new GUIStyle("ToolbarButton");
        }

        public void Set(StateGraph graph) {
            _graph = graph;
        }

        private void OnGUI() {
            if (_nodeStyles == null) {
                SetupStyles();
            }
            if (_nodeTypes.Count == 0) {
                GUILayout.Label("No Sequence Types");
            }
            _scrollRect = new Rect(0, 0, position.width-Border, position.height);
            _scrollPosition = GUI.BeginScrollView(_scrollRect, _scrollPosition, new Rect(0, 0, MaxRectSize, MaxRectSize));
            
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            DrawConnections();
            DrawNodes();
            DrawConnectionLine(Event.current);
            GUI.EndScrollView();
            
            var sideRect = new Rect(position.width - Border, 0, Border-10, position.height);
            _scrollPosition1 = GUI.BeginScrollView(sideRect, _scrollPosition1, new Rect(0, 0, Border, MaxRectSize));
            for (int i = 0; i < _graph.GlobalTriggers.Count; i++) {
                _graph.GlobalTriggers[i].Key = GUILayout.TextField(_graph.GlobalTriggers[i].Key);
            }
            if (GUILayout.Button("Add")) {
                _graph.GlobalTriggers.Add(new GraphTrigger(){ Key = "Trigger"});
            }
            GUI.EndScrollView();

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
            for (int i = 0; i < _graph.Nodes.Count; i++) {
                var node = _graph.Nodes[i];
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
                    if (Handles.Button(
                        (connectIn.Rect.center + connectOut.Rect.center) * 0.5f, Quaternion.identity,
                        4, 8, Handles.RectangleHandleCap)) {
                        OnClickRemoveConnection(connectOut);
                    }
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

        private void DrawNodes() {
            if (_graph == null) {
                return;
            }
            var animationLabels = AnimationEvents.GetValues();
            var tagLabels = GraphNodeTags.GetNames().ToArray();
            var tagValues = GraphNodeTags.GetValues();
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
                else if (node.HasConditions) {
                    styleIndex = node.AllowEarlyExit ? StyleHasEarlyExit : StyleHasConditions;
                }
                var style = node == _selected ? _nodeSelectedStyles[styleIndex] : _nodeStyles[styleIndex];
                GUILayout.BeginArea(node.Rect, style);
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(10);
                GUILayout.Label(node.Title, StateGraphExtensions.NodeTextStyle, GUILayout.MaxWidth(maxWidth));
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                var tagIndex = System.Array.IndexOf(tagValues, node.Tag);
                var newTag = UnityEditor.EditorGUILayout.Popup(tagIndex, tagLabels, GUILayout.MaxWidth(maxWidth / 2));
                if (newTag != tagIndex) {
                    node.Tag = tagValues[newTag];
                }
                GUILayout.Label("ID: " + node.Id, StateGraphExtensions.NodeTextStyle, GUILayout.MaxWidth(maxWidth/2));
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                if (node.DrawGui(StateGraphExtensions.NodeTextStyle, _nodeButtonStyle)) {
                    Repaint();
                }
                for (int c = 0; c < node.Conditions.Count; c++) {
                    node.Conditions[c].DrawGui(node, StateGraphExtensions.NodeTextStyle, _nodeButtonStyle);
                    GUILayout.Space(10);
                }
                if (node.Conditions.Count > 0 && node.OutPoints.Count > 1) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Else exit ", StateGraphExtensions.NodeTextStyle);
                    var indices = new string[node.OutPoints.Count];
                    for (int idx = 0; idx < indices.Length; idx++) {
                        indices[idx] = idx.ToString();
                    }
                    node.DefaultExit = UnityEditor.EditorGUILayout.Popup(node.DefaultExit, indices, StateGraphExtensions.NodeTextStyle);
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                var enterIndex = System.Array.IndexOf(animationLabels, node.EnterEvent);
                var exitIndex = System.Array.IndexOf(animationLabels, node.ExitEvent);
                GUILayout.Label("Enter");
                var newEnter = UnityEditor.EditorGUILayout.Popup(enterIndex, animationLabels);
                if (newEnter != enterIndex) {
                    node.EnterEvent = animationLabels[newEnter];
                }
                GUILayout.Label("Exit");
                var newExit = UnityEditor.EditorGUILayout.Popup(exitIndex, animationLabels);
                if (newExit != exitIndex) {
                    node.ExitEvent = animationLabels[newExit];
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                if (node.Conditions.Count < node.MaxConditions) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("Add Condition", _nodeButtonStyle)) {
                        node.Conditions.Add(new ConditionExit());
                        node.CheckSize();
                    }
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(node);
                }
                GUILayout.EndArea();
                var inSpacing = Mathf.Clamp(0.8f/ node.InPoints.Count, 0.02f, 1);
                for (int c = 0; c < node.InPoints.Count; c++) {
                    if (node.InPoints[c] == null) {
                        node.InPoints.RemoveAt(c);
                        break;
                    }
                    StateGraphExtensions.DrawConnectionPoint(node.InPoints[c], c, inSpacing, OnClickInPoint, RemoveConnectionPoint);
                }
                var outSpacing = Mathf.Clamp(0.8f / node.OutPoints.Count, 0.02f, 1);
                for (int c = 0; c < node.OutPoints.Count; c++) {
                    if (node.OutPoints[c] == null) {
                        node.OutPoints.RemoveAt(c);
                        break;
                    }
                    StateGraphExtensions.DrawConnectionPoint(node.OutPoints[c], c, outSpacing, OnClickOutPoint, RemoveConnectionPoint);
                }
                if (node.InPoints.Count < node.InputMax) {
                    var addRect = new Rect(
                        node.Rect.x + ConnectionOutPoint.Width * 0.5f,
                        node.Rect.y + node.Rect.height - (ConnectionOutPoint.Height*1.25f),
                        ConnectionOutPoint.Width, ConnectionOutPoint.Height);
                    if (GUI.Button(addRect, "+", StateGraphExtensions.NodeTextStyle)) {
                        node.InPoints.Add(new ConnectionInPoint(node, node.FindMinConnectionId()));
                        EditorUtility.SetDirty(node);
                    }
                }
                if (node.OutPoints.Count < node.OutputMax) {
                    var addRect = new Rect(
                        node.Rect.x + node.Rect.width - (ConnectionOutPoint.Width*1.25f),
                        node.Rect.y + node.Rect.height - (ConnectionOutPoint.Height*1.25f),
                        ConnectionOutPoint.Width, ConnectionOutPoint.Height);
                    if (GUI.Button(addRect, "+", StateGraphExtensions.NodeTextStyle)) {
                        node.OutPoints.Add(new ConnectionOutPoint(node, node.FindMinConnectionId()));
                        EditorUtility.SetDirty(node);
                    }
                }
            }
        }

        private void RemoveConnectionPoint(ConnectionOutPoint point) {
            //_graph.ClearConnectsWith(point);
//            for (int i = _graph.Connections.Count - 1; i >= 0; i--) {
//                if (_graph.Connections[i].GetIn() == point || _graph.Connections[i].GetOut() == point) {
//                    _graph.Connections.RemoveAt(i);
//                }
//            }
            point.Owner.OutPoints.Remove(point);
            EditorUtility.SetDirty(point.Owner);
        }

        private void RemoveConnectionPoint(ConnectionInPoint point) {
            _graph.ClearConnectsWith(point);
//            for (int i = _graph.Connections.Count - 1; i >= 0; i--) {
//                if (_graph.Connections[i].GetIn() == point || _graph.Connections[i].GetOut() == point) {
//                    _graph.Connections.RemoveAt(i);
//                }
//            }
            point.Owner.InPoints.Remove(point);
            EditorUtility.SetDirty(point.Owner);
        }
        
        
        private void ProcessNodeEvents(Event e) {
            if (_graph == null) {
                return;
            }
            _mouseScrollPosition = e.mousePosition + _scrollPosition;
            for (int i = _graph.Nodes.Count - 1; i >= 0; i--) {
                var node = _graph.Nodes[i];
                bool guiChanged = false;
                switch (e.type) {
                    case EventType.MouseDown:
                        switch (e.button) {
                            case 0:
                                if (node.Rect.Contains(_mouseScrollPosition)) {
                                    _selected = _dragged = node;
                                    GUI.changed = true;
                                }
                                else if (_selected == node) {
                                    _selected = null;
                                    GUI.changed = true;
                                }
                                break;
                            case 1:
                                if (node.Rect.Contains(_mouseScrollPosition)) {
                                    _selected = node;
                                    GUI.changed = true;
                                    GenericMenu genericMenu = new GenericMenu();
                                    genericMenu.AddItem(new GUIContent("Remove node"), false, () => OnClickRemoveNode(node));
                                    genericMenu.AddItem(new GUIContent("Toggle Default"), false, () => OnClickSetDefault(node));
                                    genericMenu.AddItem(new GUIContent("Duplicate"), false, () => OnDuplicate(node));
                                    genericMenu.AddItem(new GUIContent("CheckSize"), false, () => node.CheckSize());
                                    genericMenu.AddItem(new GUIContent(node.AllowEarlyExit? "Disable Early Exit" : "Enable Early Exit"), false, () => 
                                    OnSetEarlyExit(node));
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
                        if (e.button == 0 && _dragged == node && e.clickCount < 2) {
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
                _scrollPosition += -Vector2.ClampMagnitude(delta, 50);
                Event.current.Use();
            }
//            _drag = Vector2.ClampMagnitude(delta, MaxDrag);
//            if (Event.current.button == 2 && Event.current.type == EventType.MouseDrag) {
//                _scrollPosition += -Event.current.delta;
//                Event.current.Use();
//            }
//            if (_graph != null) {
//                for (int i = 0; i < _graph.Nodes.Count; i++) {
//                    _graph.Nodes[i].Drag(_drag);
//                    EditorUtility.SetDirty(_graph.Nodes[i]);
//                }
//            }
//            GUI.changed = true;
        }
        
        private void ClearConnectionSelection() {
            _selectedInPoint = null;
            _selectedOutPoint = null;
        }

        private void CreateConnection() {
            _selectedOutPoint.Target = _selectedInPoint.Owner;
            _selectedOutPoint.TargetId = _selectedInPoint.Id;
        }

        private void OnClickInPoint(ConnectionInPoint inPoint) {
            _selectedInPoint = inPoint;
            if (_selectedOutPoint != null) {
                if (_selectedOutPoint.Owner != _selectedInPoint.Owner) {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(ConnectionOutPoint outPoint) {
            _selectedOutPoint = outPoint;
            if (_selectedInPoint != null) {
                if (_selectedOutPoint.Owner != _selectedInPoint.Owner) {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveConnection(ConnectionOutPoint outPnt) {
            outPnt.Target = null;
            outPnt.TargetId = -1;
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

        private void OnSetEarlyExit(StateGraphNode node) {
            node.AllowEarlyExit = !node.AllowEarlyExit;
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

        private void OnDuplicate(StateGraphNode node) {
            var newObj = Instantiate(node);
            AssetDatabase.AddObjectToAsset(newObj, _graph);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
            SetupNewNode(newObj);
            newObj.CheckSize();
        }

        private void CreateContextItem(object obj) {
            var targetType = obj as Type;
            if (targetType == null) {
                return;
            }
            var newObj = CreateInstance(targetType);
            AssetDatabase.AddObjectToAsset(newObj, _graph);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
            SetupNewNode((StateGraphNode) newObj);
        }

        private void SetupNewNode(StateGraphNode node) {
            _graph.Nodes.Add(node);
            node.Set(_mouseScrollPosition, GetUniqueId(), _graph);
            EditorUtility.SetDirty(_graph);
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

    public static class StateGraphExtensions {
        private static GUIStyle _inPointStyle;
        private static GUIStyle _outPointStyle;
        private static GUIStyle _nodeTextStyle;
        
        public static GUIStyle NodeTextStyle {
            get {
                if (!_setup) {
                    Init();
                }
                return _nodeTextStyle;
            }
        }
        
        private static bool _setup = false;

        private static void Init() {
            _setup = true;
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
        
        public static void DrawConnectionPoint(ConnectionOutPoint point, int index, float spacing, Action<ConnectionOutPoint> onClickOut,
         Action<ConnectionOutPoint> onRemove) {
            if (!_setup) {
                Init();
            }
            var height = (point.Owner.Rect.height * ((index + 1) * spacing));
            point.Rect.y = point.Owner.Rect.y + height - point.Rect.height * 0.5f;
            point.Rect.x = point.Owner.Rect.x + point.Owner.Rect.width - 8f;
            if (GUI.Button(point.Rect, "", _outPointStyle)) {
                onClickOut?.Invoke(point);
            }
            var labelRect = new Rect(point.Rect);
            GUI.Label(labelRect, " " + index.ToString(), _nodeTextStyle);
            var buttonRect = new Rect(point.Rect);
            if (point.Owner.OutPoints.Count <= point.Owner.OutputMin) {
                return;
            }
            buttonRect.x -= point.Rect.width + 1;
            if (GUI.Button(buttonRect, "-", _nodeTextStyle)) {
                onRemove?.Invoke(point);
            }
        }

        public static void DrawConnectionPoint(ConnectionInPoint point, int index, float spacing, Action<ConnectionInPoint> onClickPoint,
            Action<ConnectionInPoint> onRemove) {
            if (!_setup) {
                Init();
            }
            var height = (point.Owner.Rect.height * ((index + 1) * spacing));
            point.Rect.y = point.Owner.Rect.y + height - point.Rect.height * 0.5f;
            point.Rect.x = point.Owner.Rect.x - point.Rect.width + 8f;
            if (GUI.Button(point.Rect, "", _inPointStyle)) {
                onClickPoint?.Invoke(point);
            }
            var labelRect = new Rect(point.Rect);
            GUI.Label(labelRect, " " + index.ToString(), _nodeTextStyle);
            var buttonRect = new Rect(point.Rect);
            if (point.Owner.InPoints.Count <= point.Owner.InputMin) {
                return;
            }
            buttonRect.x += point.Rect.width + 1;
            if (GUI.Button(buttonRect, "-", _nodeTextStyle)) {
                onRemove?.Invoke(point);
            }
        }
    }
}