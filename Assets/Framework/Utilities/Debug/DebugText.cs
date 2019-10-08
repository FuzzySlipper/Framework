using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace PixelComrades {
    public class DebugText : MonoSingleton<DebugText> {

        [SerializeField] private bool _startEnabled = true;

        private GenericPool<TextTimer> _textTimers = new GenericPool<TextTimer>(10);
        private List<TextTimer> _tempText = new List<TextTimer>();
        private Dictionary<string, Text> _permTextDict = new Dictionary<string, Text>();
        private List<Line> _lines = new List<Line>();
        private RingBuffer<string> _threadedText = new RingBuffer<string>(20);
        private CanvasGroup _canvasGroup;
        private Transform _permRoot;
        private Transform _tempRoot;

        private CanvasGroup Canvas {
            get {
                if (_canvasGroup == null) {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        private Transform PermRoot {
            get {
                if (_permRoot == null) {
                    _permRoot = transform.Find("PermText");
                }
                return _permRoot;
            }
        }

        private Transform TempRoot {
            get {
                if (_tempRoot == null) {
                    _tempRoot = transform.Find("TempText");
                }
                return _tempRoot;
            }
        }

        private static string _lastText = "";

        public static void Log(string text, float length = 2f) {
            if (!Application.isPlaying) {
                Debug.Log(text);
                return;
            }
            if (main == null) {
                return;
            }
            if (_lastText == text) {
                return;
            }
            _lastText = text;
            var newText = main._textTimers.New();
            var textObject = ItemPool.SpawnUIPrefab("UI/DebugTextTemp", main.TempRoot);
            textObject.GetComponent<Text>().text = text;
            newText.Time.StartNewTime(length);
            newText.TextObject = textObject;
            main._tempText.Add(newText);
        }

        public static void UpdatePermText(string label, string text) {
            if (!Application.isPlaying) {
                Debug.Log(text);
                return;
            }
            if (main == null) {
                return;
            }
            Text objText;
            if (!main._permTextDict.TryGetValue(label, out objText)) {
                var textObject = ItemPool.SpawnUIPrefab("UI/DebugTextPerm", main.PermRoot);
                objText = textObject.GetComponent<Text>();
                main._permTextDict.Add(label, objText);
            }
            objText.text = string.Format("{0} - {1}", label, text);
        }

        public static void RemovePermText(string label) {
            Text objText;
            if (!main._permTextDict.TryGetValue(label, out objText)) {
                return;
            }
            ItemPool.Despawn(objText.gameObject);
            main._permTextDict[label] = null;
            main._permTextDict.Remove(label);
        }

        public static void AddLine(Vector3 start, Vector3 end, Color color, float length) {
            if (start == end) {
                return;
            }
            main._lines.Add(new Line(start, end, color, length));
        }

        public static void LogThreading(string text) {
            if (main == null) {
                return;
            }
            main._threadedText.Enqueue(text);
        }
        
        public static bool IsActive { get { return main.Canvas.alpha > 0; } }

        [Command("DebugTextToggle")]
        public static void Toggle() {
            if (IsActive) {
                main.Canvas.alpha = 0;
            }
            else {
                main.Canvas.alpha = 1;
            }
        }

        void Awake() {
            main = this;
            //if (Application.isEditor) {
            //    main.Canvas.alpha = 1;
            //}
            if (Application.isPlaying && Application.isEditor && _startEnabled) {
                Toggle();
            }
        }

        void Update() {
            if (Application.isPlaying && Canvas.alpha > 0.5f) {
                UpdatePermText("Global", string.Format("Active {0} Paused {1} PausedMod {2}", Game.GameActive, Game.Paused, Game.PauseHolder.LastId()));
            }
            for (int i = _tempText.Count - 1; i >= 0; i--) {
                if (!_tempText[i].Time.IsActive) {
                    ItemPool.Despawn(_tempText[i].TextObject);
                    _tempText[i].TextObject = null;
                    _textTimers.Store(_tempText[i]);
                    _tempText.RemoveAt(i);
                }
            }
            if (_threadedText.Count > 0) {
                var count = _threadedText.Count;
                string text;
                for (int i = 0; i < count; i++) {
                    if (_threadedText.TryDequeue(out text)) {
                        Log(text);
                    }
                }
            }
        }
#if UNITY_EDITOR
        void OnDrawGizmos() {
            for (int i = _lines.Count - 1; i >= 0; i--) {
                //Debug.DrawLine(_lines[i].Start, _lines[i].End, _lines[i].Color);
                //Gizmos.color = _lines[i].Color;
                //Gizmos.DrawLine(_lines[i].Start, _lines[i].End);
                //Gizmos.DrawSphere(_lines[i].Start, 1);
                //Gizmos.DrawSphere(_lines[i].End, 1);
                DrawArrow.ForGizmo(_lines[i].Start, (_lines[i].End - _lines[i].Start), _lines[i].Color, 1.5f);
                if (TimeManager.Time > _lines[i].EndTime) {
                    _lines.RemoveAt(i);
                }
            }
        }
#endif

        [System.Serializable] public struct Line {
            public Vector3 Start;
            public Vector3 End;
            public Color Color;
            public float EndTime;

            public Line(Vector3 start, Vector3 end, Color color, float length) {
                Start = start;
                End = end;
                Color = color;
                EndTime = TimeManager.Time + length;
            }
        }


        [System.Serializable] public class TextTimer {
            public ScaledTimer Time = new ScaledTimer(2);
            public GameObject TextObject;
        }

    }
#if UNITY_EDITOR
    public static class DrawArrow {
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            if (direction == Vector3.zero) {
                return;
            }
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForHandle(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            if (direction == Vector3.zero) {
                return;
            }
            UnityEditor.Handles.DrawLine(pos, pos + direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            var trStart = pos + direction;
            UnityEditor.Handles.DrawLine(trStart, trStart + right * arrowHeadLength);
            UnityEditor.Handles.DrawLine(trStart, trStart + left * arrowHeadLength);
        }

        public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
#endif
}