using System.Collections;
using PixelComrades;
using UnityEngine;
using UnityEngine.EventSystems;

public class FirstPersonController : MonoSingleton<FirstPersonController>, ISystemFixedUpdate, IOnCreate {
    
    //add kicker volume to be dragged around below to move debris
    public static Transform Tr { get; private set; }
    public static bool Climbing { get { return main.CurrentState.Label == FpState.Labels.Climb; } }
    public static bool Jumping  { get { return main.CurrentState.Label == FpState.Labels.Jump; } }
    public static bool Dodging { get { return main.CurrentState.Label == FpState.Labels.Dodge; } }
    public static bool Grounded { get { return main._hoverPower > 0; } }
    public static bool Moving { get; private set; }
    public static bool Running { get; set; }

    public Vector2 MoveInput;

    [SerializeField] private FpControllerSettings _settings = null;
    //[SerializeField] private FirstPersonAnimator _animator = null;
    [SerializeField] private CapsuleCollider _collider = null;

    private AudioSource _audio;
    private Rigidbody _rigidbody;
    private float _nextStep;
    private bool _previouslyGrounded;
    private RaycastHit _groundHit;
    private Vector3 _moveDirection;
    private bool _enabled = false;
    private float _leftGround = 0;
    private float _hoverPower;
    private ScaledTimer _jumpTimer = new ScaledTimer(0.25f);


    private StateMachine<FirstPersonController> _machine;

    private FpState CurrentState {get { return _machine.CurrentState as FpState; } }
    public SurfaceDetector.SurfaceData CurrentSurface { get; set; }
    public bool Unscaled {get { return false; }}
    public bool InputRun { get; set; }
    public Vector3 Velocity {get { return _rigidbody.velocity; } }
    public FpControllerSettings Settings { get { return _settings; } }
    public StateMachine<FirstPersonController> Machine { get { return _machine; } }
    public AudioSource Audio { get { return _audio; } }
    //public FirstPersonAnimator Animator { get { return _animator; } }
    public Rigidbody Rb { get { return _rigidbody; } }
    public bool CanMove { get; set; }
    public Collider Collider { get { return _collider; } }
    public float Height { get { return _collider.height; } set { _collider.height = value; } }
    public float Radius { get { return _collider.radius; } set { _collider.radius = value; } }
    public Vector3 Center { get { return _collider.center; } set { _collider.center = value; } }
    public bool ControlledMove { get { return Dodging || Jumping  || Climbing  || CurrentState.VerticalMoving; } }
    public Vector3 MoveDirection { get { return _moveDirection; } }

    public void OnCreate(PrefabEntity entity) {
        Tr = transform;
        _rigidbody = GetComponent<Rigidbody>();
        Player.Rb = _rigidbody;
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = false;
        _audio.spatialBlend = 1f;

        _rigidbody.SetPhysics(false);
        _machine = new StateMachine<FirstPersonController>(this, new FpNormalState());
        _machine.AddState(new FpClimbState());
        _machine.AddState(new FpFallState());
        _machine.AddState(new FpJumpState());
        _machine.AddState(new FpDodgeState());
        CanMove = false;
        _enabled = false;
        MessageKit.addObserver(Messages.LoadingFinished, EnablePlayer);
        MessageKit.addObserver(Messages.Loading, DisablePlayer);
    }

    private void DisablePlayer() {
        _rigidbody.SetPhysics(false);
        CanMove = false;
        _enabled = false;
    }

    private void EnablePlayer() {
        //Grounded = true;
        CanMove = true;
        _previouslyGrounded = true;
        _enabled = true;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50, LayerMasks.Environment)) {
            transform.position = hit.point + new Vector3(0, _collider.radius,0);
        }
        _rigidbody.SetPhysics(true);
        _machine.ChangeState<FpNormalState>();
    }

    public void TryJump() {
        if (!TryClimb()) {
            Jump();
        }
    }

    public void Jump() {
        if (ControlledMove|| !Grounded || _jumpTimer.IsActive) {
            return;
        }
        _jumpTimer.Activate();
        if (Mathf.Abs(MoveInput.x) >= _settings.DodgeMovePercent || 
            MoveInput.y <= -_settings.DodgeMovePercent) {
            Dodge();
            return;
        }
        ProcessJump();
    }

    private void ProcessJump() {
        SurfaceDetector.main.PlayJumpingSound(this);
        Rb.drag = 0f;
        var force = Settings.JumpForce - _rigidbody.velocity.y;
        if (force > 0) {
            _rigidbody.AddForce(new Vector3(0f,force, 0f), ForceMode.VelocityChange);
        }
    }

    public void Dodge() {
        if (ControlledMove || !Grounded) {
            return;
        }
        //if (Player.Actor.VitalStats[Vitals.Energy].Current < 5) {
        //    return;
        //}
        //Player.Actor.VitalStats[Vitals.Energy].Current -= 5;
        _machine.ChangeState<FpDodgeState>();
    }

    public void PlayerDie() {
        enabled = false;
    }

    public bool TryClimb() {
        if (Jumping || Dodging || Climbing) {
            return false;
        }
        var origin = transform.position + (-transform.forward)*0.5f;
        var ray = new Ray(origin, transform.forward);
        if (!Physics.SphereCast(ray, _collider.radius, _settings.ClimbDistance, LayerMasks.Environment)) {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 5f);
            return false;
        }
        var limit = transform.position.y + _collider.radius*_settings.ClimbHeightMulti;
        while (ray.origin.y < limit) {
            ray.origin += _settings.ClimbStep;
            RaycastHit hit;
            if (!Physics.SphereCast(ray, _collider.radius, out hit, _settings.ClimbDistance, LayerMasks.Environment)) {
                Debug.DrawRay(ray.origin, ray.direction, Color.green, 5f);
                var pos = ray.origin + ray.direction*_settings.ClimbDistance;
                _machine.ChangeState<FpClimbState>();
                (_machine.CurrentState as FpClimbState).Pos = pos;
                return true;
            }
            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 5f);
        }
        return false;
    }

    public void DisconnectAllGrapple() {
        //if (AnchorItemMesh.Current != null) {
        //    AnchorItemMesh.Current.Disconnect();
        //}
    }

    public void OnFixedSystemUpdate(float delta) {
        if (!_enabled || !Game.GameActive) {
            return;
        }
        GroundCheck();
        _rigidbody.drag = Grounded ? _settings.GroundDrag : _settings.AirDrag;
        Movement();
        //_animator.UpdateMoving(Moving ? 1 : 0);
        PlayFootStepAudio();
    }

    private void Movement() {
        if (!CanMove) {
            Moving = false;
            return;
        }
        var horizontal = MoveInput.x;
        var vertical = MoveInput.y;
        var movingForward = vertical > 0f;

        vertical *= movingForward ? 1f : _settings.BackwardsSpeed;
        horizontal *= _settings.SidewaysSpeed;

        var screenMovementSpace = Quaternion.Euler(0f, FirstPersonCamera.Tr.eulerAngles.y, 0f);
        var forwardVector = screenMovementSpace*Vector3.forward*vertical;
        var rightVector = screenMovementSpace*Vector3.right*horizontal;

        var moveVector = forwardVector + rightVector;

        CurrentState.UpdateMovement(moveVector, movingForward, ref _moveDirection);
        Move();
        transform.rotation = Quaternion.Lerp(transform.rotation, screenMovementSpace,
            _settings.RotationSpeed*TimeManager.DeltaUnscaled);
        Moving = Grounded && Velocity.magnitude > 0.15f;
    }

    //private void AdjustDrag() {
    //    if (!Grounded) {
    //        _rigidbody.drag = 0;
    //        if (_previouslyGrounded && !Jumping) {
    //            StickToGroundHelper();
    //        }
    //        return;
    //    }
    //    _rigidbody.drag = 5f;
    //}

    private void Move() {
        if (_moveDirection == Vector3.zero) {
            return;
        }
        if (_rigidbody.velocity.sqrMagnitude < CurrentState.MoveSpeed*CurrentState.MoveSpeed) {
            //_rigidbody.AddForce(moveDir*SlopeMultiplier(), ForceMode.VelocityChange);
            _rigidbody.AddForce(_moveDirection, ForceMode.VelocityChange);
        }
    }
    
    private float SlopeMultiplier() {
        var angle = Vector3.Angle(_groundHit.normal, Vector3.up);
        return _settings.SlopeCurveModifier.Evaluate(angle);
    }

    ///// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    //private void GroundCheck() {
    //    _previouslyGrounded = Grounded;
    //    RaycastHit hitInfo;
    //    if (Physics.SphereCast(transform.position + Center, _collider.radius, Vector3.down, out hitInfo,
    //        _collider.height/2f - _collider.radius + _settings.GroundCheckDistance, LayerMasks.Environment)) {
    //        Grounded = true;
    //        _groundContactNormal = hitInfo.normal;
    //    }
    //    else {
    //        if (_previouslyGrounded) {
    //            _leftGround = Global.Time;
    //        }
    //        Grounded = false;
    //        _groundContactNormal = Vector3.up;
    //    }
    //    //AdjustDrag();
    //}
    
    //private void StickToGroundHelper() {
    //    RaycastHit hitInfo;
    //    if (Physics.SphereCast(transform.position, _collider.radius, Vector3.down, out hitInfo,
    //        _collider.height/2f - _collider.radius +
    //        _settings.StickToGroundHelperDistance, LayerMasks.Environment)) {
    //        if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f) {
    //            _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, hitInfo.normal);
    //        }
    //    }
    //}

    private bool CheckGround(Vector3 up) {
        if (Physics.Raycast(transform.position, -up, out _groundHit, _settings.MaxHeightForHover, LayerMasks.Environment)) {
            return true;
        }
        if (Settings.LookAheadGroundingDistance > 0) {
            var pos = transform.position + (transform.forward*Settings.LookAheadGroundingDistance);
            if (Physics.Raycast(pos, -up, out _groundHit, _settings.MaxHeightForHover, LayerMasks.Environment)) {
                return true;
            }
        }
        Debug.DrawRay(transform.position, -up, Color.red, 1.5f);
        if (_previouslyGrounded) {
            _leftGround = TimeManager.Time;
        }
        return false;
    }
    
    private void GroundCheck() {
        _previouslyGrounded = Grounded;
        var up = transform.up; // find force direction by rotating local up vector towards world up
        var grav = Physics.gravity.normalized;
        up = Vector3.RotateTowards(up, -grav, _settings.MaxHoverAngleDrift*Mathf.Deg2Rad, 1);
        _hoverPower = 0;
        if (!CheckGround(up)) {
            return;
        }
        // calculate power falloff
        _hoverPower = Mathf.Pow((_settings.MaxHeightForHover - _groundHit.distance)/
            _settings.MaxHeightForHover, _settings.ExponentHeightForce);
        if (ControlledMove) {
            return;
        }
        var force = _hoverPower*_settings.MaxGroundForce;
        // calculate damping, which is proportional to square of upward velocity
        var v = Vector3.Dot(_rigidbody.GetPointVelocity(transform.position), up);
        var drag = -v*Mathf.Abs(v)*_settings.HoverDamping;
        // add force and damping
        _rigidbody.AddForceAtPosition(up*(force + drag), transform.position);
    }

    private void PlayFootStepAudio() {
        if (!_previouslyGrounded && Grounded && (TimeManager.Time - _leftGround > _settings.MinTimeForFall)) {
            SurfaceDetector.main.PlayLandingSound(this);
            _nextStep = CameraHeadBob.HeadBobCycle + _settings.StepInterval;
            _moveDirection.y = 0f;
            return;
        }
        if (CameraHeadBob.HeadBobCycle > _nextStep) {
            _nextStep = CameraHeadBob.HeadBobCycle + _settings.StepInterval;
            if (Grounded) {
                SurfaceDetector.main.PlayFootStepSound(this);
            }
            if (Climbing) {
                //_currentLadder.PlayLadderFootstepSound(ref _audio, _settings.SoundsVolume);
            }
        }
    }

#if UNITY_EDITOR
    void OnGUI() {
        if (!Game.GameActive) {
            return;
        }
        var right = Screen.width - 350;
        var start = 20;
        //GUI.Label(new Rect(right, start, 100, 25),
        //    "Active " +  Active.Count);
        GUI.Label(new Rect(right, start, 150, 25),
            "Can Move" + CanMove);
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "velocity" + _rigidbody.velocity.ToString("F1"));
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "ControlledMove" + ControlledMove);
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "Dir" + _moveDirection);
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "State" + CurrentState.Description);
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "Ground" + Grounded);
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "Cursor" + Cursor.lockState.ToString());
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "UIActive" + UIRadialMenu.Active);
        start += 35;
        GUI.Label(new Rect(right, start, 150, 25),
            "Pointer" + EventSystem.current.IsPointerOverGameObject());
    }



    protected void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }
        UnityEditor.Handles.Label(transform.position, Machine.CurrentState.Description);
        Machine.CurrentState.OnGizmo();
    }

#endif

    
    //private void OnControllerColliderHit(ControllerColliderHit hit) {
    //    if (!_controller.enabled) {
    //        return;
    //    }
    //    if (_collisionFlags != CollisionFlags.Below) {
    //        var hitbody = hit.collider.attachedRigidbody;
    //        if (hitbody && !hitbody.isKinematic) {
    //            hitbody.AddForceAtPosition(hit.moveDirection, hit.point, ForceMode.Impulse);
    //        }
    //    }
    //}

    //private void OnTriggerEnter(Collider other) {
    //    if (!_controller.enabled) {
    //        return;
    //    }
    //    if (!_crouching && other.CompareTag("Ladder")) {
    //        if (Crouched) {
    //            _crouching = true;
    //            TimeManager.Start(StandUp());
    //        }
    //        _currentLadder = other.GetComponent<Ladder>();
    //        _moveDirection = Vector3.zero;
    //        Climbing = true;
    //    }
    //}

    //private void OnTriggerExit(Collider other) {
    //    if (!_controller.enabled) {
    //        return;
    //    }
    //    if (!_crouching && other.CompareTag("Ladder")) {
    //        Climbing = false;
    //        _currentLadder = null;
    //    }
    //}

}
