using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class DebugRenderingSystem : SystemBase, IMainLateUpdate {

        public const int DrawLineLayer = 4;
        public static Color DrawDefaultColor = Color.white;

        private List<DrawTextEntry> _drawTextEntries;
        private List<AttachTextEntry> _attachTextEntries;
        private GUIStyle _textStyle;
        
        private List<DrawLineEntry> _lineEntries;
        private BatchedLineDraw _zTestBatch;
        private BatchedLineDraw _alwaysBatch;
        private bool _linesNeedRebuild;

        public DebugRenderingSystem() {
            _zTestBatch = new BatchedLineDraw(depthTest: true);
            _alwaysBatch = new BatchedLineDraw(depthTest: false);
            _lineEntries = new List<DrawLineEntry>(16);

            _textStyle = new GUIStyle();
            _textStyle.alignment = TextAnchor.UpperLeft;
            _drawTextEntries = new List<DrawTextEntry>(16);
            _attachTextEntries = new List<AttachTextEntry>(16);
        }

        public void OnSystemLateUpdate(float dt, float unscaledDt) {
            TickAndDrawLines();
        }

        public override void Dispose() {
            base.Dispose();
            _alwaysBatch.Dispose();
            _zTestBatch.Dispose();

        }

        public void Clear() {
            _drawTextEntries.Clear();
            _lineEntries.Clear();
            _linesNeedRebuild = true;
        }
        
        private void RebuildDrawLineBatchMesh() {
            _zTestBatch.Clear();
            _alwaysBatch.Clear();

            for (int ix = 0; ix < _lineEntries.Count; ix++) {
                var entry = _lineEntries[ix];
                if (!entry.occupied)
                    continue;

                if (entry.noZTest)
                    _alwaysBatch.AddLine(entry.start, entry.end, entry.color);
                else
                    _zTestBatch.AddLine(entry.start, entry.end, entry.color);
            }

            _zTestBatch.BuildBatch();
            _alwaysBatch.BuildBatch();
        }

        private void TickAndDrawLines() {
            if (_linesNeedRebuild) {
                RebuildDrawLineBatchMesh();
                _linesNeedRebuild = false;
            }

            //	draw on UI layer which should bypass most postFX setups
            Graphics.DrawMesh(
                _alwaysBatch.mesh, Vector3.zero, Quaternion.identity, _alwaysBatch.mat,  LayerMasks.NumberDefault, null,
                0, null, false, false);
            Graphics.DrawMesh(
                _zTestBatch.mesh, Vector3.zero, Quaternion.identity, _zTestBatch.mat, LayerMasks.NumberDefault, null,
                0, null, false, false);

            //	update timer late so every added entry can be drawed for at least one frame
            for (int ix = 0; ix < _lineEntries.Count; ix++) {
                var entry = _lineEntries[ix];
                if (!entry.occupied)
                    continue;
                entry.timer -= Time.deltaTime;
                if (entry.timer < 0) {
                    entry.occupied = false;
                    _linesNeedRebuild = true;
                }
            }

            return;
        }

        public void RegisterLine(Vector3 start, Vector3 end, Color color, float timer, bool noZTest) {
            DrawLineEntry entry = null;
            for (int ix = 0; ix < _lineEntries.Count; ix++) {
                if (!_lineEntries[ix].occupied) {
                    entry = _lineEntries[ix];
                    break;
                }
            }
            if (entry == null) {
                entry = new DrawLineEntry();
                _lineEntries.Add(entry);
            }

            entry.occupied = true;
            entry.start = start;
            entry.end = end;
            entry.color = color;
            entry.timer = timer;
            entry.noZTest = noZTest;
            _linesNeedRebuild = true;
        }
        public void RegisterDrawText(Vector3 anchor, string text, Color color, int size, float timer, bool popUp)
		{
			DrawTextEntry entry = null;
			for (int ix = 0; ix < _drawTextEntries.Count; ix++)
			{
				if (!_drawTextEntries[ix].occupied)
				{
					entry = _drawTextEntries[ix];
					break;
				}
			}
			if (entry == null)
			{
				entry = new DrawTextEntry();
				_drawTextEntries.Add(entry);
			}

			entry.occupied = true;
			entry.anchor = anchor;
			entry.content.text = text;
			entry.size = size;
			entry.color = color;
			entry.duration = entry.timer = timer;
			entry.popUp = popUp;
#if UNITY_EDITOR
			entry.flag = DrawFlag.None;
#else
			//	in builds consider gizmo is already drawn
			entry.flag = DrawFlag.DrawnGizmo;
#endif

			return;
		}

		public void RegisterAttachText(Transform target, Func<string> strFunc, Vector3 offset, Color color, int size)
		{
			AttachTextEntry entry = null;
			for (int ix = 0; ix < _attachTextEntries.Count; ix++)
			{
				if (!_attachTextEntries[ix].occupied)
				{
					entry = _attachTextEntries[ix];
					break;
				}
			}
			if (entry == null)
			{
				entry = new AttachTextEntry();
				_attachTextEntries.Add(entry);
			}

			entry.occupied = true;
			entry.offset = offset;
			entry.transform = target;
			entry.strFunc = strFunc;
			entry.color = color;
			entry.size = size;
			//	get first text
			entry.content.text = strFunc();
#if UNITY_EDITOR
			entry.flag = DrawFlag.None;
#else
			//	in builds consider gizmo is already drawn
			entry.flag = DrawFlag.DrawnGizmo;
#endif

			return;
		}

		private void TickTexts()
		{
			for (int ix = 0; ix < _drawTextEntries.Count; ix++)
			{
				var entry = _drawTextEntries[ix];
				if (!entry.occupied)
					continue;
				entry.timer -= Time.deltaTime;
				if (entry.flag == DrawFlag.DrawnAll)
				{
					if (entry.timer < 0)
					{
						entry.occupied = false;
					}
					//	actually no need to tick DrawFlag as it won't move
				}
			}

			for (int ix = 0; ix < _attachTextEntries.Count; ix++)
			{
				var entry = _attachTextEntries[ix];
				if (!entry.occupied)
					continue;
				if (entry.transform == null)
				{
					entry.occupied = false;
					entry.strFunc = null;	// needs to release ref to callback
				}
				else if (entry.flag == DrawFlag.DrawnAll)
				{
					// tick content
					entry.content.text = entry.strFunc();
					// tick flag
#if UNITY_EDITOR
					entry.flag = DrawFlag.None;
#else
					//	in builds consider gizmo is already drawn
					entry.flag = DrawFlag.DrawnGizmo;
#endif
				}
			}


			return;
		}

		private void DrawTextOnGUI()
		{
			var camera = Camera.main;
			if (camera == null)
				return;

			for (int ix = 0; ix < _drawTextEntries.Count; ix++)
			{
				var entry = _drawTextEntries[ix];
				if (!entry.occupied)
					continue;

				GUIDrawTextEntry(camera, entry);
				entry.flag |= DrawFlag.DrawnGUI;
			}

			for (int ix = 0; ix < _attachTextEntries.Count; ix++)
			{
				var entry = _attachTextEntries[ix];
				if (!entry.occupied)
					continue;

				GUIAttachTextEntry(camera, entry);
				entry.flag |= DrawFlag.DrawnGUI;
			}

			return;
		}

		private void GUIDrawTextEntry(Camera camera, DrawTextEntry entry)
		{
			Vector3 worldPos = entry.anchor;
			Vector3 screenPos = camera.WorldToScreenPoint(worldPos);
			screenPos.y = Screen.height - screenPos.y;

			if (entry.popUp)
			{
				float ratio = entry.timer / entry.duration;
				screenPos.y -=  (1 - ratio * ratio) * entry.size * 1.5f;
			}

			_textStyle.normal.textColor = entry.color;
			_textStyle.fontSize = entry.size;
			Rect rect = new Rect(screenPos, _textStyle.CalcSize(entry.content));
			GUI.Label(rect, entry.content, _textStyle);

			return;
		}

		private void GUIAttachTextEntry(Camera camera, AttachTextEntry entry)
		{
			if (entry.transform == null)
				return;

			Vector3 worldPos = entry.transform.position + entry.offset;
			Vector3 screenPos = camera.WorldToScreenPoint(worldPos);
			screenPos.y = Screen.height - screenPos.y;

			_textStyle.normal.textColor = entry.color;
			_textStyle.fontSize = entry.size;
			Rect rect = new Rect(screenPos, _textStyle.CalcSize(entry.content));
			GUI.Label(rect, entry.content, _textStyle);

			return;
		}

        private class DrawLineEntry {
            public bool occupied;
            public Vector3 start;
            public Vector3 end;
            public Color color;
            public float timer;
            public bool noZTest;
        }

        [Flags]
        public enum DrawFlag : byte {
            None = 0,
            DrawnGizmo = 1 << 0,
            DrawnGUI = 1 << 1,
            DrawnAll = DrawnGizmo | DrawnGUI
        }
        
        private class DrawTextEntry {
            public bool occupied;
            public GUIContent content;
            public Vector3 anchor;
            public int size;
            public Color color;
            public float timer;
            public bool popUp;
            public float duration;

            //	Text entries needs to be draw in both OnGUI/OnDrawGizmos, need flags for mark
            //	has been visited by both
            public DrawFlag flag = DrawFlag.None;

            public DrawTextEntry() {
                content = new GUIContent();
                return;
            }
        }

        private class AttachTextEntry {
            public bool occupied;
            public GUIContent content;
            public Vector3 offset;
            public int size;
            public Color color;


            public Transform transform;
            public Func<string> strFunc;

            public DrawFlag flag = DrawFlag.None;

            public AttachTextEntry() {
                content = new GUIContent();
                return;
            }
        }

        
        private class BatchedLineDraw : IDisposable {
            public Mesh mesh;
            public Material mat;

            private List<Vector3> _vertices;
            private List<Color> _colors;
            private List<int> _indices;

            public BatchedLineDraw(bool depthTest) {
                mesh = new Mesh();
                mesh.MarkDynamic();

                //	relying on a builtin shader, but it shouldn't change that much.
                mat = new Material(Shader.Find("Hidden/Internal-Colored"));
                mat.SetInt("_ZTest", depthTest
                        ? 4 // LEqual
                        : 0 // Always
                );

                _vertices = new List<Vector3>();
                _colors = new List<Color>();
                _indices = new List<int>();
            }

            public void AddLine(Vector3 from, Vector3 to, Color color) {
                _vertices.Add(from);
                _vertices.Add(to);
                _colors.Add(color);
                _colors.Add(color);
                int verticeCount = _vertices.Count;
                _indices.Add(verticeCount - 2);
                _indices.Add(verticeCount - 1);
            }

            public void Clear() {
                mesh.Clear();
                _vertices.Clear();
                _colors.Clear();
                _indices.Clear();
            }

            public void BuildBatch() {
                mesh.SetVertices(_vertices);
                mesh.SetColors(_colors);
                mesh.SetIndices(_indices.ToArray(), MeshTopology.Lines, 0); // cant get rid of this alloc for now
            }

            public void Dispose() {
                UnityEngine.Object.DestroyImmediate(mesh);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
    }
}
