using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PixelComrades {
    public abstract class GraphController : EditorWindow {
        public static GraphController Current;

        public GraphCamera Camera;
        public bool ForceSnapping = true;
        public Vector2 MousePosGlobal; // Global mouse position based on camera offsets
        public Vector2 MousePos; // Local screen mouse position
        public IGridNode SelectedNode;

        protected bool IsTransition; // Are we in transistion mode
        protected int SelectIndex = -1; // Currently selected window

        private float _sidebarWidth = 140f; // Size of the sidebar
        private GUIStyle _textStyle; // Style used for title in upper left
        private GraphSidebar _sidebar;

        protected abstract string GetTitle { get; }
        protected abstract string GetWindowTitle { get; }
        protected abstract GraphSidebar CreateSidebar();
        protected abstract GraphCamera CreateCamera();
        protected abstract Vector2 GridCellSize { get; }
        protected abstract int NodeCount { get; }
        protected abstract IGridNode GetNode(int index);
        protected abstract bool LeftClickMenu(Event e);
        protected abstract bool RightClickMenu(Event e);
        protected abstract void DrawNodeWindows();

        protected virtual void BeginGroupTransition() {
            IsTransition = true;
            SelectedNode = GetNode(SelectIndex);
        }

        protected virtual void DeleteGroupTransition(object obj) {
        }
        
        protected virtual void EndGroupTransition() {
            IsTransition = false;
            SelectedNode = null;
        }

        private void OnEnable() {
            titleContent.text = GetWindowTitle;
            _textStyle = new GUIStyle();
            _textStyle.fontSize = 20;
            if (_sidebar == null) {
                _sidebar = CreateSidebar();
            }
            if (Camera == null) {
                Camera = CreateCamera();
            }
            Current = this;
            UpdateTarget(Selection.activeGameObject);
        }

        private void OnGUI() {
            DrawTitle();

            var e = Event.current;
            MousePos = e.mousePosition; // Mouse position local to the viewing window
            MousePosGlobal = Camera.GetMouseGlobal(e.mousePosition); // Mouse position local to the scroll window
            Camera.Offset = GUI.BeginScrollView(new Rect(0f, 0f, position.width - _sidebarWidth, position.height), Camera.Offset,
                new Rect(Camera.ViewportSize.x / -2f, Camera.ViewportSize.y / -2f, Camera.ViewportSize.x, Camera.ViewportSize.y));
            if (e.type == EventType.Repaint) {
                DrawGrid(0);
            }
            // Clicked outside of sidebar and scrollbar GUI
            if (MousePos.x < position.width - _sidebarWidth - GUI.skin.verticalScrollbar.fixedWidth &&
                MousePos.y < position.height - GUI.skin.horizontalScrollbar.fixedHeight) {
                if (e.button == 1 && !IsTransition) {
                    if (e.type == EventType.MouseDown) {
                        RightClickMenu(e);
                    }
                }
                else if (e.button == 0) {
                    LeftClickMenu(e);
                }
            }
            BeginWindows();
            DrawNodeWindows();
            EndWindows();
            GUI.EndScrollView(); // Camera scroll for windows
            // Draw a transistion if in transistion mode
            if (IsTransition && SelectedNode != null) {
                var globalOffset = Camera.GetOffsetGlobal();
                var beginRect = SelectedNode.EditorWindowRect;
                beginRect.x -= globalOffset.x;
                beginRect.y -= globalOffset.y;
                var mouseRect = new Rect(MousePos.x, MousePos.y, 10f, 10f);

                DrawNodeCurve(beginRect, mouseRect, Color.black);

                Repaint();
            }
            _sidebar.DrawSidebar(new Rect(position.width - _sidebarWidth, 0, _sidebarWidth, position.height), 10f,
                Color.gray, this);

            // Always stop the camera on mouse up (even if not in the window)
            if (Event.current.rawType == EventType.MouseUp) {
                Camera.EndMove();
            }

            // Poll and update the viewport if the camera has moved
            if (Camera.PollCamera(MousePos)) {
                Repaint();
            }
        }
        
        private void DrawTitle() {
            var targetDisplayName = GetTitle;
            GUI.Label(new Rect(10, 10, 100, 20), targetDisplayName, _textStyle);
        }

        private void OnSelectionChange() {
            UpdateTarget(Selection.activeGameObject);
        }

        protected virtual void UpdateTarget(GameObject go) {
            Camera.Reset();
            Repaint();
        }

        public static void DrawLineBottomToTop(Rect start, Rect end, Color color) {
            var startPos = new Vector3(start.x + start.width / 2f, start.y + start.height, 0f);
            var endPos = new Vector3(end.x + end.width / 2f, end.y, 0f);
            var startTan = startPos + Vector3.up * 50f;
            var endTan = endPos - Vector3.up * 50f;
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 3f);
        }

        public static void DrawNodeCurve(Rect start, Rect end, Color color) {
            var startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
            var endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
            var startTan = startPos + Vector3.down * 50;
            var endTan = endPos + Vector3.up * 50;
            var shadowCol = new Color(0, 0, 0, 0.06f);

            for (var i = 0; i < 3; i++) {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 1);
        }

        public static void DrawNodeLine(Rect start, Rect end, Color color) {
            var startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
            var endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
            var startTan = startPos + Vector3.down * 50;
            var endTan = endPos + Vector3.up * 50;
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 1);
        }

        private void DrawGrid(float height) {
            Handles.color = new Color(0.27f, 0.27f, 0.27f);
            var xMin = -Camera.ViewportSize.x;
            var xMax = Camera.ViewportSize.x;
            var yMin = -Camera.ViewportSize.y;
            var yMax = Camera.ViewportSize.y;
            for (var x = xMin; x <= xMax+10; x+= GridCellSize.x) {
                var xPos = x;
                Handles.DrawLine(new Vector3(xPos, yMin, height),
                    new Vector3(xPos, yMax, height));
            }
            for (var y = yMin; y <= yMax+10; y+=GridCellSize.y) {
                var yPos = y;
                Handles.DrawLine(new Vector3(xMin, yPos, height),
                    new Vector3(xMax, yPos, height));
            }
            Handles.color = Color.white;
        }


        public void SnapAllNodesToGrid() {
            for (int i = 0; i < NodeCount; i++) {
                SnapNodeToGrid(GetNode(i));
            }
        }

        public virtual void SnapNodeToGrid(IGridNode node) {
            var pos = GetSnappedPosition(node.EditorWindowRect.position, node.EditorWindowRect.size);
            node.EditorWindowRect = new Rect(pos, node.EditorWindowRect.size);
        }

        protected Vector2 GetSnappedPosition(Vector2 currentPos, Vector2 size) {
            var pos = new Vector2(Mathf.Round(currentPos.x / GridCellSize.x) * GridCellSize.x,
                Mathf.Round(currentPos.y / GridCellSize.y) * GridCellSize.y);
            var xOffset = GridCellSize.x - size.x;
            var yOffset = GridCellSize.y - size.y;
            pos.x += xOffset * 0.5f;
            pos.y += yOffset * 0.5f;
            return pos;
        }
        
        //public Point3 EditorToWorld(Vector2 pos) {
        //    return new Point3((int) Math.Round((double) pos.x/(TileSizeWide/Camera.ViewportSize.x)), 0,
        //        (int) Math.Round((double) pos.y/(TileSizeHeight/Camera.ViewportSize.x)));
        //}
    }

    public class TransitionParentChild {
        public IGridNode Child;
        public IGridNode Parent;
    }
}