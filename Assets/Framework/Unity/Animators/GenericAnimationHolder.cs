using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace PixelComrades {
    public interface IRuntimeAnimationHolder {
        float Remaining { get; }
        float CurrentTime { get; }
        float Length { get; }
        GenericAnimation Animation { get; }
        void SetTimeNormalized(float normalized);
        void SetTime(float time);
        void Play();
        void Stop();
        void Update(float dt);
        void Dispose();
    }
    public class GenericAnimationHolder : IRuntimeAnimationHolder {
        
        private List<IRuntimeAnimationObject> _allObjects = new List<IRuntimeAnimationObject>();
        private List<IRuntimeAnimationObject> _pendingObjects = new List<IRuntimeAnimationObject>();
        private List<IRuntimeAnimationObject> _currentObjects = new List<IRuntimeAnimationObject>();
        private float _currentTime;
        
        public float Remaining { get => Length - _currentTime; }
        public float CurrentTime { get => _currentTime; }
        public float Length { get; }
        public GenericAnimation Animation { get; private set; }

        public GenericAnimationHolder(GenericAnimation owner) {
            Animation = owner;
            Length = owner.FindEndTime();
            for (int i = 0; i < Animation.Objects.Count; i++) {
                _allObjects.Add(Animation.Objects[i].GetRuntime(this));
            }
        }

        public void SetTimeNormalized(float normalized) {
            _currentTime = Length * normalized;
            CheckAnimationList(true);
        }

        public void SetTime(float time) {
            _currentTime = time;
            CheckAnimationList(true);
        }

        public void Play() {
            Stop();
            _pendingObjects.AddRange(_allObjects);
        }
        
        public void Stop() {
            _currentObjects.Clear();
            _pendingObjects.Clear();
            _currentTime = 0;
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
                    var newObject = _currentObjects[i];
                    newObject.OnEnter();
                    _pendingObjects.RemoveAt(i);
                    _currentObjects.Add(newObject);
                }
            }
        }

        public void Update(float dt) {
            _currentTime += dt;
            for (int i = 0; i < _currentObjects.Count; i++) {
                _currentObjects[i].OnUpdate(dt);
            }
            CheckAnimationList(false);
        }

        public void Dispose() {
            Stop();
            for (int i = 0; i < Animation.Objects.Count; i++) {
                Animation.Objects[i].DisposeRuntime(_allObjects[i]);
            }
            _allObjects.Clear();
            Animation = null;
        }
    }
}
