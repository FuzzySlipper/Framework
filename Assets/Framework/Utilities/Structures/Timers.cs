using System;
using UnityEngine;

namespace PixelComrades {



    public class LerpHolder {
        private float _percent;
        private float _targetValue;
        private float _startValue;
        private float _duration = 0;
        private float _startTime = 0;
        private bool _unscaled = false;

        public bool Enabled { get; set; }
        public float Percent { get { return _percent; } }
        public float StartTime { get { return _startTime; } }
        public float Duration { get { return _duration; } }
        public float CurrentTime { get { return _unscaled ? TimeManager.TimeUnscaled : TimeManager.Time; } }

        public void RestartLerp(float start, float target, float duration, bool unscaled = false) {
            _startValue = start;
            _targetValue = target;
            _duration = duration;
            _percent = 0;
            _unscaled = unscaled;
            _startTime = CurrentTime;
            Enabled = true;
        }

        public float GetLerpValue() {
            if (Math.Abs(_startTime) < 0.001f || Math.Abs(_duration) < 0.001f) {
                return 0;
            }
            if (_percent >= 1) {
                return _targetValue;
            }
            _percent = (CurrentTime - _startTime) / _duration;
            return Mathf.Lerp(_startValue, _targetValue, _percent);
        }

        public void Cancel() {
            _percent = 1;
        }

        public bool IsFinished { get { return _percent >= 1; } }
    }

    public class LerpTr : LerpHolder {
        private Vector3 _endPos;
        private Vector3 _startPos;
        private Quaternion _endRotation;
        private Quaternion _startRotation;
        private bool _local;
        private Transform _tr;

        public void SetTarget(Vector3 end, Quaternion endRot, Transform tr, float duration) {
            _startPos = tr.position;
            _startRotation = tr.rotation;
            _endPos = end;
            _endRotation = endRot;
            RestartLerp(0, 1, duration);
            _local = false;
            _tr = tr;
        }

        public void SetTargetLocal(Vector3 end, Quaternion endRot, Transform tr, float duration) {
            _startPos = tr.localPosition;
            _startRotation = tr.localRotation;
            _endPos = end;
            _endRotation = endRot;
            RestartLerp(0, 1, duration);
            _local = true;
            _tr = tr;
        }

        public void UpdatePosition() {
            if (_local) {
                _tr.localPosition = Vector3.Lerp(_startPos, _endPos, GetLerpValue());
                _tr.localRotation = Quaternion.Slerp(_startRotation, _endRotation, GetLerpValue());
            }
            else {
                _tr.position = Vector3.Lerp(_startPos, _endPos, GetLerpValue());
                _tr.rotation = Quaternion.Slerp(_startRotation, _endRotation, GetLerpValue());
            }
        }
    }

    [System.Serializable]
    public class Timer {

        [SerializeField] private float _length = 0;
        [SerializeField, HideInInspector] private bool _activated = false;
        [SerializeField, HideInInspector] private bool _unscaled = false;
        [SerializeField, HideInInspector] private float _start = 0;
        
        public float Length { get { return _length; } }

        public Timer(float length, bool unscaled) {
            _length = length;
            _unscaled = unscaled;
        }

        public Timer() {
        }

        private float Time { get { return _unscaled ? TimeManager.TimeUnscaled : TimeManager.Time; } }
        public float TimeLeft { get { return _activated ? (_start + Length) - Time : 0; } }
        public bool IsActive { get { return _activated && Time <= _start + Length; } }
        public float Percent { get { return _activated ? (Time - _start) / Length : 1; } }

        public void Restart() {
            _start = Time;
            _activated = true;
        }

        public void Cancel() {
            _start = 0;
            _activated = false;
        }

        public void StartNewTime(float length, bool unscaled) {
            _length = length;
            _activated = true;
            _unscaled = unscaled;
            _start = Time;
        }

        public void StartNewTime(float length) {
            _length = length;
            _activated = true;
            _start = Time;
        }
    }


    [System.Serializable]
    public class ScaledTimer {

        [SerializeField] private float _length = 0;
        [SerializeField, HideInInspector] private bool _activated = false;
        [SerializeField, HideInInspector] private float _start = 0;
        public float Length { get { return _length; } }
        
        public ScaledTimer(float length) {
            _length = length;
        }

        public ScaledTimer() {
        }

        public float TimeLeft { get { return _activated ? (_start + Length) - TimeManager.Time : 0; } }
        public bool IsActive { get { return _activated && TimeManager.Time <= _start + Length; } }
        public float Percent { get { return _activated ? (TimeManager.Time - _start) / Length : 1; } }
        public bool Activated { get { return _activated; } }

        public void Activate() {
            _start = TimeManager.Time;
            _activated = true;
        }

        public void Cancel() {
            _start = 0;
            _activated = false;
        }

        public void RestartTimer() {
            _activated = true;
            _start = TimeManager.Time;
        }

        public void StartNewTime(float length) {
            _length = length;
            _activated = true;
            _start = TimeManager.Time;
        }
    }

    [System.Serializable]
    public class UnscaledTimer {
        
        [SerializeField, HideInInspector] private bool _activated = false;
        [SerializeField, HideInInspector] private float _start = 0;
        [SerializeField] private float _length = 0;
        public float Length { get { return _length; } }
        public UnscaledTimer(float length) {
            _length = length;
        }

        public UnscaledTimer() {
        }

        public float TimeLeft { get { return _activated ? (_start + Length) - TimeManager.TimeUnscaled : 0; } }
        public bool IsActive { get { return _activated && TimeManager.TimeUnscaled < _start + Length; } }
        public float Percent { get { return _activated ? (TimeManager.TimeUnscaled - _start) / Length : 1; } }

        public virtual void StartTimer() {
            _start = TimeManager.TimeUnscaled;
            _activated = true;
        }

        public void Cancel() {
            _start = 0;
            _activated = false;
        }

        public virtual void StartNewTime(float length) {
            _length = length;
            _activated = true;
            _start = TimeManager.TimeUnscaled;
        }
    }

    [System.Serializable]
    public class TriggerableUnscaledTimer : UnscaledTimer {
        
        [SerializeField, HideInInspector] private bool _triggered = false;
        public bool Triggered { get { return _triggered; } set { _triggered = value; } }

        public override void StartNewTime(float length) {
            base.StartNewTime(length);
            _triggered = true;
        }

        public override void StartTimer() {
            base.StartTimer();
            _triggered = true;
        }
    }

    [System.Serializable]
    public class TurnTimer {

        [SerializeField, HideInInspector] private bool _active;
        [SerializeField, HideInInspector] private int _start = 0;
        [SerializeField] private int _length = 0;
        public int Length { get { return _length; } }
        public int TimeLeft { get { return _active ? (_start + Length) - TurnBased.TurnNumber : 0; } }
        public float Percent { get { return _active ? (TurnBased.TurnNumber - _start) / Length : 1; } }
        public bool IsActive { get { return _active && TimeLeft > 0; } }

        public void UpdateCount() {
            _active = TurnBased.TurnNumber <= _start + Length;
        }

        public void Cancel() {
            _start = 0;
            _active = false;
        }

        public TurnTimer() {
        }

        public TurnTimer(int length) {
            _length = length;
        }

        public void StartTime(int length) {
            _length = length;
            _active = true;
            _start = TurnBased.TurnNumber;
        }
    }
}