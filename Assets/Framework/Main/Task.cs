using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class Task {
        public enum UpdateMode {
            None,
            Frame,
            Time,
            Task
        }

        public event Action OnFinish;
        public string Id { get; private set; }
        public bool Unscaled { get; private set; }
        public float Current { get { return _current; } }
        public UpdateMode Mode { get { return _mode; } }
        public float WaitFor { get { return _waitFor; } }

        private IEnumerator _routine = null;
        private UpdateMode _mode = UpdateMode.None;
        private float _waitFor = 0f;
        private float _current = 0;
        private bool _isFinished = false;

        public void Set(IEnumerator routine, string taskId, bool unscaled) {
            _isFinished = false;
            _routine = routine;
            Id = taskId;
            Unscaled = unscaled;
        }

        public bool Finished {
            get { return _isFinished; }
            private set {
                _isFinished = value;
                if (value && OnFinish != null) {
                    OnFinish();
                    OnFinish = null;
                }
            }
        }

        public void Clear() {
            Finished = true;
            Unscaled = false;
            _routine = null;
            _current = 0;
            _waitFor = 0f;
            Id = "Cleared";
            _mode = UpdateMode.None;
        }

        public void Cancel() {
            TimeManager.Cancel(this);
        }

        public void UpdateTask(float deltaTime) {
            switch (_mode) {
                case UpdateMode.Task:
                    return;
                case UpdateMode.Time:
                    _current += deltaTime;
                    break;
                default:
                    _current += 1;
                    break;
            }
            if (_current >= _waitFor) {
                UpdateRoutine();
            }
        }

        private void WatchedTaskFinished() {
            if (_mode != UpdateMode.Task) {
                return;
            }
            _current += 1;
            if (_current > _waitFor) {
                UpdateRoutine();
            }
        }

        private void UpdateRoutine() {
            _current = 0;
            if (_routine == null || !_routine.MoveNext() || Finished) {
                Finished = true;
                return;
            }
            if (_routine == null) {
                Debug.LogError(Id + " hit null");
                return;
            }
            if (_routine.Current == null) {
                _waitFor = 1;
                _mode = UpdateMode.Frame;
                return;
            }
            var yieldCommand = _routine.Current;
            if (yieldCommand is int) {
                _waitFor = (int) yieldCommand;
                _mode = UpdateMode.Frame;
                return;
            }
            if (yieldCommand is float) {
                _waitFor = (float) yieldCommand;
                _mode = UpdateMode.Time;
                return;
            }
            IEnumerator routine = yieldCommand as IEnumerator;
            Task waitTask;
            if (routine != null) {
                waitTask = TimeManager.Start(routine, Unscaled);
            }
            else {
                waitTask = yieldCommand as Task;
            }
            if (waitTask != null) {
                _mode = UpdateMode.Task;
                waitTask.OnFinish += WatchedTaskFinished;
                _waitFor = 0;
                return;
            }
            var taskArray = yieldCommand as IList<Task>;
            if (taskArray != null) {
                _mode = UpdateMode.Task;
                for (int i = 0; i < taskArray.Count; i++) {
                    taskArray[i].OnFinish += WatchedTaskFinished;
                }
                _waitFor = taskArray.Count - 1;
                return;
            }
            Debug.LogError(string.Format("Unexpected yield type: {0} in {1}", yieldCommand.GetType(), Id));
            Finished = true;
        }
    }
}