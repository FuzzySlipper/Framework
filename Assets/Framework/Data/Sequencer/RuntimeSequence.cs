using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace PixelComrades {
    public class RuntimeSequence : IRuntimeSequence {

        public static RuntimeSequence DebugSequence;
        
        private List<IRuntimeSequenceObject> _allObjects = new List<IRuntimeSequenceObject>();
        private List<IRuntimeSequenceObject> _pendingObjects = new List<IRuntimeSequenceObject>();
        private List<IRuntimeSequenceObject> _currentObjects = new List<IRuntimeSequenceObject>();
        private float _currentTime;
        
        public float Remaining { get => Length - _currentTime; }
        public float CurrentTime { get => _currentTime; }
        public float Length { get; }
        public GenericSequence Sequence { get; private set; }
        public bool IsComplete { get; private set; }
        public Entity Entity { get; private set; }

        public RuntimeSequence(Entity owner, GenericSequence sequence) {
            Entity = owner;
            Sequence = sequence;
            Length = sequence.FindEndTime();
            for (int i = 0; i < Sequence.Objects.Count; i++) {
                _allObjects.Add(Sequence.Objects[i].GetRuntime(this));
            }
        }

        public void SetEntity(Entity owner) {
            Entity = owner;
        }

        public void SetTimeNormalized(float normalized) {
            _currentTime = Length * normalized;
            CheckAnimationList(true);
        }

        public void SetTime(float time) {
            Play();
            _currentTime = time;
            for (int i = _pendingObjects.Count - 1; i >= 0; i--) {
                var obj = _pendingObjects[i];
                if (obj.EndTime <= _currentTime) {
                    _pendingObjects.RemoveAt(i);
                    continue;
                }
                if (obj.StartTime <= _currentTime) {
                    StartSequence(obj);
                    _pendingObjects.RemoveAt(i);
                }
            }
        }

        public void Play() {
            Stop();
            IsComplete = false;
            _pendingObjects.AddRange(_allObjects);
        }

        private void Reset() {
            _currentObjects.Clear();
            _pendingObjects.Clear();
        }
        
        public void Stop() {
            Reset();
            _currentTime = 0;
        }

        public void Update(float dt) {
            if (IsComplete) {
                return;
            }
            _currentTime += dt;
            for (int i = 0; i < _currentObjects.Count; i++) {
                _currentObjects[i].OnUpdate(dt);
            }
            CheckAnimationList(false);
            if (_currentObjects.Count == 0 && _pendingObjects.Count == 0) {
                IsComplete = true;
            }
        }
        
        private void CheckAnimationList(bool removeEarly) {
            for (int i = _currentObjects.Count - 1; i >= 0; i--) {
                var currentObject = _currentObjects[i];
                if (_currentTime >= currentObject.EndTime) {
                    currentObject.OnExit();
                    _currentObjects.RemoveAt(i);
                }
                if (removeEarly && _currentTime < currentObject.StartTime) {
                    _pendingObjects.Add(currentObject);
                    _currentObjects.RemoveAt(i);
                }
            }
            for (int i = _pendingObjects.Count - 1; i >= 0; i--) {
                if (_currentTime >= _pendingObjects[i].StartTime) {
                    var newObject = _pendingObjects[i];
                    _pendingObjects.RemoveAt(i);
                    StartSequence(newObject);
                }
            }
        }

        private void StartSequence(IRuntimeSequenceObject newObject) {
            newObject.OnEnter();
            _currentObjects.Add(newObject);
        }


        public void Dispose() {
            Stop();
            for (int i = 0; i < _allObjects.Count; i++) {
                _allObjects[i].Dispose();
            }
            _allObjects.Clear();
            Sequence = null;
        }
    }

    public interface IRuntimeSequence {
        Entity Entity { get; }
        float CurrentTime { get; }
        GenericSequence Sequence { get; }
        void Play();
        void Stop();
        void Update(float dt);
        void Dispose();
    }
}
