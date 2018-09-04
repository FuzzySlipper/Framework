using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelComrades {
    public class TimeManager : MonoBehaviour {

        private static TimeManager _main;

        private static TimeManager main {
            get {
                if (_main == null) {
                    _main = FindObjectOfType<TimeManager>();
                }
                if (_main == null) {
                    _main = Game.MainObject.AddComponent<TimeManager>();
                }
                return _main;
            }
            set { _main = value; }
        }

        private static float _deltaTime = 0.001f;
        private static float _fixedDelta = 0.001f;
        private static float _fixedDeltaUnscaled = 0.001f;
        private static float _deltaUnscaled = 0.001f;
        private static ScaledTimer _pathTimer = new ScaledTimer(0.1f);
        private static float _time = 0;
        private static float _timeUnscaled = 0;
        private static float _timeScale = 1;

        public static float DeltaTime { get { return _deltaTime; } }
        public static float FixedDelta { get { return _fixedDelta; } }
        public static float FixedDeltaUnscaled { get { return _fixedDeltaUnscaled; } }
        public static float DeltaUnscaled { get { return _deltaUnscaled; } }
        public static bool IsQuitting { get; private set; }

        private GenericPool<Task> _taskPool = new GenericPool<Task>(50, item => item.Clear());
        private List<Task> _active = new List<Task>();
        private float _lastTime = 0;
#if UNITY_EDITOR
        private bool _editorUpdate = false;
#endif
        public static float TimeScale { get { return Game.Paused ? 0 : _timeScale; } }

        public static float Time {
            get {
                if (!Application.isPlaying) {
                    return UnityEngine.Time.realtimeSinceStartup;
                }
                return _time;
            }
        }
        public static float TimeUnscaled {
            get {
                if (!Application.isPlaying) {
                    return UnityEngine.Time.realtimeSinceStartup;
                }
                return _timeUnscaled;
            }
        }
        public int ActiveCount { get { return _active.Count; } }
        public Task this[int index] { get { return index < _active.Count ? _active[index] : null; } }

        public static Task Start(IEnumerator routine, bool unscaled, System.Action del = null) {
            return main.StartTask(routine, routine.ToString(), unscaled, del);
        }

        public static Task Start(IEnumerator routine, System.Action del = null) {
            if (!main) {
                return null;
            }
            return main.StartTask(routine, routine.ToString(), false, del);
        }

        public static Task StartUnscaled(IEnumerator routine, System.Action del = null) {
            if (!main) {
                return null;
            }
            return main.StartTask(routine, routine.ToString(), true, del);
        }

        public static void PauseFor(float time, bool unScaled, System.Action onComplete) {
            main.StartTask(main.GenericPause(time), "GenericPause", unScaled, onComplete);
        }

        public static void PauseFrame(System.Action onComplete) {
            main.StartTask(main.GenericPause(), "GenericPause", true, onComplete);
        }

        private IEnumerator GenericPause(float time) {
            yield return time;
        }

        private IEnumerator GenericPause() {
            yield return null;
        }

        public static void Cancel(Task task) {
            main.CancelInternal(task);
        }

        public static void Cancel(string task) {
            main.CancelInternal(task);
        }

        public Task StartTask(IEnumerator routine, string id, bool unscaled = false, System.Action del = null) {
            var task = _taskPool.New();
            task.Set(routine, id, unscaled);
            if (del != null) {
                task.OnFinish += del;
            }
            _active.Add(task);
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                CheckEditor();
            }
#endif
            return task;
        }

        public void CancelInternal(string id) {
            for (int i = _active.Count - 1; i >= 0; i--) {
                if (_active[i].Id != id) {
                    continue;
                }
                var finishedTask = _active[i];
                _active.RemoveAt(i);
                _taskPool.Store(finishedTask);
            }
        }

        public void CancelInternal(Task task) {
            if (!_active.Contains(task)) {
                return;
            }
            _active.Remove(task);
            _taskPool.Store(task);
        }

        public void CancelAll() {
            for (int i = 0; i < _active.Count; i++) {
                var finishedTask = _active[i];
                _taskPool.Store(finishedTask);
            }
            _active.Clear();
        }

        void Awake() {
            _lastTime = 0;
            main = this;
            IsQuitting = false;
            _fixedDeltaUnscaled = UnityEngine.Time.fixedDeltaTime;
        }

        void OnApplicationQuit() {
            IsQuitting = true;
        }

        public void OnDestroy() {
            IsQuitting = true;
        }

        void Update() {
            //_time += UnityEngine.Time.deltaTime * TimeScale;
            if (!_pathTimer.IsActive) {
                _pathTimer.Activate();
            }
            _deltaTime = UnityEngine.Time.deltaTime * TimeScale;
            _deltaUnscaled = UnityEngine.Time.deltaTime;
            if (!Game.Paused) {
                _time += _deltaTime;
            }
            _timeUnscaled += _deltaUnscaled;
            //_deltaUnscaled = UnityEngine.Time.unscaledDeltaTime;
            RunUpdate();
            SystemManager.SystemUpdate();
            World.Update(_deltaTime);
            //if (GameOptions.UseCulling) {
            //    CullingManager.Main.Update();
            //}
        }

        void FixedUpdate() {
            _fixedDelta = UnityEngine.Time.fixedDeltaTime * TimeScale;
            SystemManager.FixedSystemUpdate(FixedDelta);
        }

        private void RunUpdate() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                SetDeltaEditor();
            }
#endif
            for (int i = 0; i < _active.Count; i++) {
                if (Game.Paused && !_active[i].Unscaled) {
                    continue;
                }
                _active[i].UpdateTask(_active[i].Unscaled ? DeltaUnscaled : DeltaTime);
            }
            for (int i = _active.Count - 1; i >= 0; i--) {
                if (_active[i].Finished) {
                    var finishedTask = _active[i];
                    _active.RemoveAt(i);
                    _taskPool.Store(finishedTask);
                }
            }
            _lastTime = TimeUnscaled;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                CheckEditor();
            }
#endif
        }

        private void SetDeltaEditor() {
            _time += TimeUnscaled * TimeScale;
            _deltaTime = Game.Paused ? 0 : (TimeUnscaled - _lastTime) * TimeScale;
            _deltaUnscaled = TimeUnscaled - _lastTime;
        }

#if UNITY_EDITOR
        public void CheckEditor() {
            if (Application.isPlaying) {
                if (_editorUpdate) {
                    _editorUpdate = false;
                    EditorApplication.update = null;
                }
                return;
            }
            if (_active.Count > 0 && !_editorUpdate) {
                _editorUpdate = true;
                _lastTime = Time;
                EditorApplication.update += RunUpdate;
            }
            else if (_active.Count == 0 && _editorUpdate) {
                _editorUpdate = false;
                EditorApplication.update -= RunUpdate;
            }
        }
#endif
    }
}