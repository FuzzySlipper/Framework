using System;
using UnityEngine;

namespace PixelComrades {
    //[Serializable]
    //public class SimpleSpring {
    //    public enum UpdateMode {
    //        Position,
    //        PositionAdditiveLocal,
    //        PositionAdditiveGlobal,
    //        PositionAdditiveSelf,
    //        Rotation,
    //        RotationAdditiveLocal,
    //        RotationAdditiveGlobal,
    //        Scale,
    //        ScaleAdditiveLocal
    //    }

    //    [SerializeField] private bool _autoUpdate = true;
    //    [SerializeField] private UpdateMode _mode = UpdateMode.Position;
    //    private Vector3[] _smoothForceFrame = new Vector3[120];
    //    private Transform _transform;
    //    private UpdateDelegate _updateFunc;
    //    private Vector3 _velocity = Vector3.zero;
    //    private float _velocityFadeInCap = 1.0f;
    //    private float _velocityFadeInEndTime;
    //    private float _velocityFadeInLength;
    //    public Vector3 Damping = new Vector3(0.75f, 0.75f, 0.75f);
    //    public Vector3 MaxState = new Vector3(10000, 10000, 10000);
    //    public float MaxVelocity = 10000.0f;
    //    public Vector3 MinState = new Vector3(-10000, -10000, -10000);
    //    public float MinVelocity = 0.0000001f;
    //    public Vector3 RestState = Vector3.zero;
    //    public Vector3 State = Vector3.zero;
    //    public Vector3 Stiffness = new Vector3(0.5f, 0.5f, 0.5f);

    //    public SimpleSpring() {
    //    }

    //    public SimpleSpring(Transform transform, UpdateMode mode, bool autoUpdate = true) {
    //        _mode = mode;
    //        Transform = transform;
    //        _autoUpdate = autoUpdate;
    //    }

    //    public Transform Transform {
    //        get => _transform;
    //        set {
    //            _transform = value;
    //            RefreshUpdateMode();
    //        }
    //    }

    //    private delegate void UpdateDelegate();

    //    private void AddForceInternal(Vector3 force) {
    //        force *= _velocityFadeInCap;
    //        _velocity += force;
    //        _velocity = Vector3.ClampMagnitude(_velocity, MaxVelocity);
    //        AddVelocity();
    //    }

    //    private void AddVelocity() {
    //        State += _velocity * Time.timeScale;
    //        State.x = Mathf.Clamp(State.x, MinState.x, MaxState.x);
    //        State.y = Mathf.Clamp(State.y, MinState.y, MaxState.y);
    //        State.z = Mathf.Clamp(State.z, MinState.z, MaxState.z);
    //    }

    //    private void Calculate() {
    //        if (State == RestState) {
    //            return;
    //        }
    //        _velocity += Vector3.Scale(RestState - State, Stiffness);
    //        _velocity = Vector3.Scale(_velocity, Damping);
    //        _velocity = Vector3.ClampMagnitude(_velocity, MaxVelocity);
    //        if (_velocity.sqrMagnitude > MinVelocity * MinVelocity) {
    //            AddVelocity();
    //        }
    //        else {
    //            Reset();
    //        }
    //    }

    //    private void LocalPositionUpdate() {
    //        _transform.localPosition = State;
    //    }

    //    private void None() {
    //    }

    //    private void PositionAdditiveGlobal() {
    //        _transform.position += State;
    //    }

    //    private void PositionAdditiveLocal() {
    //        _transform.localPosition += State;
    //    }

    //    private void PositionAdditiveSelf() {
    //        _transform.Translate(State, _transform);
    //    }

    //    private void RefreshUpdateMode() {
    //        _updateFunc = None;
    //        switch (_mode) {
    //            case UpdateMode.Position:
    //                State = _transform.localPosition;
    //                if (_autoUpdate) {
    //                    _updateFunc = LocalPositionUpdate;
    //                }
    //                break;
    //            case UpdateMode.Rotation:
    //                State = _transform.localEulerAngles;
    //                if (_autoUpdate) {
    //                    _updateFunc = Rotation;
    //                }
    //                break;
    //            case UpdateMode.Scale:
    //                State = _transform.localScale;
    //                if (_autoUpdate) {
    //                    _updateFunc = Scale;
    //                }
    //                break;
    //            case UpdateMode.PositionAdditiveLocal:
    //                State = _transform.localPosition;
    //                if (_autoUpdate) {
    //                    _updateFunc = PositionAdditiveLocal;
    //                }
    //                break;
    //            case UpdateMode.PositionAdditiveGlobal:
    //                State = _transform.position;
    //                if (_autoUpdate) {
    //                    _updateFunc = PositionAdditiveGlobal;
    //                }
    //                break;
    //            case UpdateMode.RotationAdditiveLocal:
    //                State = _transform.localEulerAngles;
    //                if (_autoUpdate) {
    //                    _updateFunc = RotationAdditiveLocal;
    //                }
    //                break;
    //            case UpdateMode.RotationAdditiveGlobal:
    //                State = _transform.eulerAngles;
    //                if (_autoUpdate) {
    //                    _updateFunc = RotationAdditiveGlobal;
    //                }
    //                break;
    //            case UpdateMode.PositionAdditiveSelf:
    //                State = _transform.position;
    //                if (_autoUpdate) {
    //                    _updateFunc = PositionAdditiveSelf;
    //                }
    //                break;
    //            case UpdateMode.ScaleAdditiveLocal:
    //                State = _transform.localScale;
    //                if (_autoUpdate) {
    //                    _updateFunc = ScaleAdditiveLocal;
    //                }
    //                break;
    //        }
    //        RestState = State;
    //    }

    //    private void Rotation() {
    //        _transform.localEulerAngles = State;
    //    }

    //    private void RotationAdditiveGlobal() {
    //        _transform.eulerAngles += State;
    //    }

    //    private void RotationAdditiveLocal() {
    //        _transform.localEulerAngles += State;
    //    }

    //    private void Scale() {
    //        _transform.localScale = State;
    //    }

    //    private void ScaleAdditiveLocal() {
    //        _transform.localScale += State;
    //    }

    //    public void AddForce(Vector3 force) {
    //        if (Time.timeScale < 1.0f) {
    //            AddSoftForce(force, 1);
    //        }
    //        else {
    //            AddForceInternal(force);
    //        }
    //    }

    //    public void AddSoftForce(Vector3 force, float frames) {
    //        force /= Time.timeScale;
    //        frames = Mathf.Clamp(frames, 1, 120);
    //        AddForceInternal(force / frames);
    //        for (int v = 0; v < Mathf.RoundToInt(frames) - 1; v++) {
    //            _smoothForceFrame[v] += force / frames;
    //        }
    //    }

    //    public void ForceVelocityFadeIn(float seconds) {
    //        _velocityFadeInLength = seconds;
    //        _velocityFadeInEndTime = Time.time + seconds;
    //        _velocityFadeInCap = 0.0f;
    //    }

    //    public void Reset() {
    //        _velocity = Vector3.zero;
    //        State = RestState;
    //    }

    //    public void Stop(bool includeSmoothForce = false) {
    //        _velocity = Vector3.zero;
    //        if (includeSmoothForce) {
    //            StopSmoothForce();
    //        }
    //    }

    //    public void StopSmoothForce() {
    //        for (int v = 0; v < 120; v++) {
    //            _smoothForceFrame[v] = Vector3.zero;
    //        }
    //    }

    //    public void UpdateSpring() {
    //        if (_velocityFadeInEndTime > Time.time) {
    //            _velocityFadeInCap = Mathf.Clamp01(1 - (_velocityFadeInEndTime - Time.time) / _velocityFadeInLength);
    //        }
    //        else {
    //            _velocityFadeInCap = 1.0f;
    //        }
    //        if (_smoothForceFrame[0] != Vector3.zero) {
    //            AddForceInternal(_smoothForceFrame[0]);
    //            for (int v = 0; v < 120; v++) {
    //                _smoothForceFrame[v] = v < 119 ? _smoothForceFrame[v + 1] : Vector3.zero;
    //                if (_smoothForceFrame[v] == Vector3.zero) {
    //                    break;
    //                }
    //            }
    //        }
    //        Calculate();
    //        _updateFunc();
    //    }
    //}

    [Serializable]
    public class Spring {
        [Tooltip("Damping makes the spring velocity wear off as it approaches its rest state.")]
        [Range(0, 1)]
        [SerializeField]
        private float _damping = 0.25f;
        [Tooltip("The maximum number of frames that the soft force can be spread over.")]
        [SerializeField]
        private int _maxSoftForceFrames = 120;
        [Tooltip("The maximum value of the spring.")]
        [SerializeField]
        private Vector3 _maxValue = new Vector3(10000, 10000, 10000);
        [Tooltip("The maximum value of the velocity.")]
        [SerializeField]
        private float _maxVelocity = 10000.0f;
        [Tooltip("The minimum value of the spring.")]
        [SerializeField]
        private Vector3 _minValue = new Vector3(-10000, -10000, -10000);
        [Tooltip("The minimum value of the velocity.")]
        [SerializeField]
        private float _minVelocity = 0.00001f;
        [Tooltip("Spring stiffness - or mechanical strength - determines how loosely or rigidly the spring's velocity behaves.")]
        [Range(0, 1)]
        [SerializeField]
        private float _stiffness = 0.2f;
        private bool _resting;
        private Vector3 _restValue;
        private bool _rotationalSpring;
        private Vector3[] _softForceFrames;
        private Vector3 _value;
        private Vector3 _velocity;
        //private float _velocityFadeInCap;
        //private float _velocityFadeInEndTime;
        //[Tooltip("The amount of time it takes for the velocity to have its full impact.")]
        //[SerializeField]
        //private float _velocityFadeInLength = 1;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Spring() {
        }

        /// <summary>
        ///     Two parameter constructor.
        /// </summary>
        /// <param name="stiffness">The default stiffness of the spring.</param>
        /// <param name="damping">The default damping of the spring.</param>
        public Spring(float stiffness, float damping) {
            _stiffness = stiffness;
            _damping = damping;
        }

        public bool Resting { get => _resting; }
        public Vector3 Value { get => _value; }
        public Vector3 RestValue {
            get => _restValue;
            set {
                _resting = false;
                if (_rotationalSpring) {
                    _restValue.x = MathUtility.ClampInnerAngle(value.x);
                    _restValue.y = MathUtility.ClampInnerAngle(value.y);
                    _restValue.z = MathUtility.ClampInnerAngle(value.z);
                }
                else {
                    _restValue = value;
                }
            }
        }

        /// <summary>
        ///     Adds an external velocity to the spring in one frame.
        /// </summary>
        /// <param name="force">The force to add.</param>
        private void AddForceInternal(Vector3 force) {
            //force *= _velocityFadeInCap;
            _velocity += force;
            _velocity = Vector3.ClampMagnitude(_velocity, _maxVelocity);
            if (_rotationalSpring) {
                _velocity.x = MathUtility.ClampInnerAngle(_velocity.x);
                _velocity.y = MathUtility.ClampInnerAngle(_velocity.y);
                _velocity.z = MathUtility.ClampInnerAngle(_velocity.z);
            }
            _resting = _velocity.sqrMagnitude <= _minVelocity * _minVelocity && _value == _restValue;
        }

        /// <summary>
        ///     Adds a force distributed over up to 120 frames.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to distribute the force over.</param>
        private void AddSoftForce(Vector3 force, float frames) {
            frames = Mathf.Clamp(frames, 1, _maxSoftForceFrames);
            AddForceInternal(force / frames);
            for (int v = 0; v < Mathf.RoundToInt(frames) - 1; v++) {
                _softForceFrames[v] += force / frames;
            }
        }

        /// <summary>
        ///     Performs the spring calculations.
        /// </summary>
        private void Calculate() {
            // No work is necessary if the spring is currently resting.
            if (_resting) {
                return;
            }
            // Update the velocity based on the current stiffness and damping values.
            _velocity += (_restValue - _value) * (1 - _stiffness);
            _velocity *= _damping;
            _velocity = Vector3.ClampMagnitude(_velocity, _maxVelocity);
            // Move towards the rest point.
            Move();

            // Reset the spring if the velocity is below minimum.
            if ((_restValue - _value).sqrMagnitude <= _minVelocity * _minVelocity) {
                Reset();
            }
        }

        /// <summary>
        ///     Adds the velocity to the state and clamps state between min and max values.
        /// </summary>
        private void Move() {
            _value += _velocity;
            _value.x = Mathf.Clamp(_value.x, _minValue.x, _maxValue.x);
            _value.y = Mathf.Clamp(_value.y, _minValue.y, _maxValue.y);
            _value.z = Mathf.Clamp(_value.z, _minValue.z, _maxValue.z);
        }

        /// <summary>
        ///     Adds an external velocity to the spring in one frame.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddForce(Vector3 force) {
            AddForce(force, 1);
        }

        /// <summary>
        ///     Adds an external velocity to the spring in specified number of frames. The force will either be an external or soft
        ///     force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        public void AddForce(Vector3 force, int frames) {
            if (frames > 1) {
                AddSoftForce(force, frames);
            }
            else {
                AddForceInternal(force);
            }
        }

        /// <summary>
        ///     Initializes the spring.
        /// </summary>
        /// <param name="rotationalSpring">Is the spring used for rotations?</param>
        public void Initialize(bool rotationalSpring) {
            _softForceFrames = new Vector3[_maxSoftForceFrames];
            //_velocityFadeInEndTime = Time.time + _velocityFadeInLength;
            _resting = false;
            _rotationalSpring = rotationalSpring;
            if (_rotationalSpring) {
                _restValue.x = MathUtility.ClampInnerAngle(_restValue.x);
                _restValue.y = MathUtility.ClampInnerAngle(_restValue.y);
                _restValue.z = MathUtility.ClampInnerAngle(_restValue.z);
            }
            Reset();
        }

        /// <summary>
        ///     Resets the spring velocity and resets state to the static equilibrium.
        /// </summary>
        public void Reset() {
            _value = _restValue;
            _resting = true;
            Stop(true);
        }

        /// <summary>
        ///     Stops spring velocity.
        /// </summary>
        /// <param name="includeSoftForce">Should the soft force also be stopped?</param>
        public void Stop(bool includeSoftForce) {
            _velocity = Vector3.zero;
            if (includeSoftForce) {
                for (int v = 0; v < 120; v++) {
                    _softForceFrames[v] = Vector3.zero;
                }
            }
        }

        /// <summary>
        ///     Update the spring forces.
        /// </summary>
        public void UpdateSpring() {
            // Slowly fade in the velocity at the start.
            //if (Math.Abs(_velocityFadeInCap - 1) > 0.0001f) {
            //    if (_velocityFadeInEndTime > Time.time) {
            //        _velocityFadeInCap = Mathf.Clamp01(1 - (_velocityFadeInEndTime - Time.time) / (_velocityFadeInLength / _timeScale));
            //    }
            //    else {
            //        _velocityFadeInCap = 1;
            //    }
            //}

            // Update the smooth force each frame.
            if (_softForceFrames[0] != Vector3.zero) {
                AddForceInternal(_softForceFrames[0]);
                for (int v = 0; v < _maxSoftForceFrames; v++) {
                    _softForceFrames[v] = v < _maxSoftForceFrames - 1 ? _softForceFrames[v + 1] : Vector3.zero;
                    if (_softForceFrames[v] == Vector3.zero) {
                        break;
                    }
                }
            }
            Calculate();
        }
    }

    [Serializable]
    public class SimpleSpring {
        [Tooltip("Damping makes the spring velocity wear off as it approaches its rest state.")]
        [Range(0, 1)]
        [SerializeField]
        private float _damping = 0.25f;
        [Tooltip("The maximum number of frames that the soft force can be spread over.")]
        [SerializeField]
        private int _maxSoftForceFrames = 120;
        [Tooltip("The maximum value of the velocity.")]
        [SerializeField]
        private float _maxVelocity = 10000.0f;
        [Tooltip("The minimum value of the velocity.")]
        [SerializeField]
        private float _minVelocity = 0.00001f;
        [Tooltip("Spring stiffness - or mechanical strength - determines how loosely or rigidly the spring's velocity behaves.")]
        [Range(0, 1)]
        [SerializeField]
        private float _stiffness = 0.2f;

        private bool _resting;
        private float _restValue = 0;
        private float[] _softForceFrames;
        private float _value;
        private float _velocity;
        public bool Resting { get => _resting; }
        public float Value { get => _value; }
        public SimpleSpring() {}

        public SimpleSpring(float stiffness, float damping) {
            _stiffness = stiffness;
            _damping = damping;
        }

        /// <summary>
        ///     Adds an external velocity to the spring in one frame.
        /// </summary>
        /// <param name="force">The force to add.</param>
        private void AddForceInternal(float force) {
            //force *= _velocityFadeInCap;
            _velocity += force;
            _velocity = Mathf.Clamp(_velocity, _minVelocity, _maxVelocity);
            _resting = _velocity <= _minVelocity;
        }

        /// <summary>
        ///     Adds a force distributed over up to 120 frames.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to distribute the force over.</param>
        private void AddSoftForce(float force, float frames) {
            frames = Mathf.Clamp(frames, 1, _maxSoftForceFrames);
            AddForceInternal(force / frames);
            for (int v = 0; v < Mathf.RoundToInt(frames) - 1; v++) {
                _softForceFrames[v] += force / frames;
            }
        }

        /// <summary>
        ///     Performs the spring calculations.
        /// </summary>
        private void Calculate() {
            if (_resting) {
                return;
            }
            // Update the velocity based on the current stiffness and damping values.
            _velocity += (_restValue - _value) * (1 - _stiffness);
            _velocity *= _damping;
            _velocity = Mathf.Clamp(_velocity, _minVelocity, _maxVelocity);
            // Move towards the rest point.
            Move();

            // Reset the spring if the velocity is below minimum.
            if (Mathf.Abs(_restValue - _value) <= _minVelocity) {
                Reset();
            }
        }

        /// <summary>
        ///     Adds the velocity to the state and clamps state between min and max values.
        /// </summary>
        private void Move() {
            _value += _velocity;
        }

        /// <summary>
        ///     Adds an external velocity to the spring in one frame.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public void AddForce(float force) {
            AddForce(force, 1);
        }

        /// <summary>
        ///     Adds an external velocity to the spring in specified number of frames. The force will either be an external or soft
        ///     force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        public void AddForce(float force, int frames) {
            if (frames > 1) {
                AddSoftForce(force, frames);
            }
            else {
                AddForceInternal(force);
            }
        }

        /// <summary>
        ///     Initializes the spring.
        /// </summary>
        public void Initialize() {
            _softForceFrames = new float[_maxSoftForceFrames];
            //_velocityFadeInEndTime = Time.time + _velocityFadeInLength;
            _resting = false;
            Reset();
        }

        /// <summary>
        ///     Resets the spring velocity and resets state to the static equilibrium.
        /// </summary>
        public void Reset() {
            _value = _restValue;
            _resting = true;
            Stop(true);
        }

        /// <summary>
        ///     Stops spring velocity.
        /// </summary>
        /// <param name="includeSoftForce">Should the soft force also be stopped?</param>
        public void Stop(bool includeSoftForce) {
            _velocity = 0;
            if (includeSoftForce) {
                for (int v = 0; v < 120; v++) {
                    _softForceFrames[v] = 0;
                }
            }
        }

        /// <summary>
        ///     Update the spring forces.
        /// </summary>
        public void UpdateSpring() {
            if (Math.Abs(_softForceFrames[0]) > 0.0001f) {
                AddForceInternal(_softForceFrames[0]);
                for (int v = 0; v < _maxSoftForceFrames; v++) {
                    _softForceFrames[v] = v < _maxSoftForceFrames - 1 ? _softForceFrames[v + 1] : 0;
                    if (Math.Abs(_softForceFrames[v]) < 0.0001f) {
                        break;
                    }
                }
            }
            Calculate();
        }
    }

    public static class MathUtility {
        public static float ClampInnerAngle(float angle) {
            if (angle < -180) {
                angle += 360;
            }
            if (angle > 180) {
                angle -= 360;
            }
            return angle;
        }
    }
}