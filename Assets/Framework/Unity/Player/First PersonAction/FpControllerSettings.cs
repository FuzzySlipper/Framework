using UnityEngine;
using System.Collections;

public class FpControllerSettings : ScriptableObject {

    [Range(0,1)] public float BackwardsSpeed = .6f;
    [Range(1,7)] public float WalkSpeed = 4.25f;
    [Range(0,1)] public float SidewaysSpeed = .7f;
    [Range(0,1)] public float InAirSpeed = .35f;
    [Range(5,15)] public float RunSpeed = 12.75f;
    //[Range(15f,50)] public float RotationSpeed = 1f;
    [Range(0.1f,30f)] public float RotationSpeed = 0.2f;

    [Range(5,25)] public float JumpForce = 8f;
    [Range(0.5f, 5)] public float DodgeForce = 8f;
    public EasingTypes DodgeEase = EasingTypes.ExponentialOut;
    [Range(0.1f, 2f)] public float DodgeLength = 2f;
    [Range(0,1)] public float ClimbingSpeed = .8f;

    [Range(1,1.75f)] public float CrouchHeight = 1.25f;
    [Range(0,1)] public float SlowSpeedModifier = .45f;

    [Range(1,10)] public float FallingDamageMultiplier = 3.5f;
    [Range(5,15)] public float FallingDistanceToDamage = 4f;
    [Range(1,5)] public float GravityMultiplier = 2f;
    [Range(0,10)] public float FallExtraForce = 2f;

    [Range(0,1)] public float SoundsVolume = .75f;
    [Range(0.1f,1.5f)] public float StepInterval = .5f;

    [Range(0,1)] public float PosForce = .65f;
    [Range(0,1)] public float TiltForce = .85f;

    [Range(1f,10)] public float ShakeForceBase = 5;
    [Range(0.5f,3)] public float ShakeLength = 2;
    public EasingTypes ShakeEase = EasingTypes.BounceInOut;
    public bool AirControl; // can the user control the direction that is being moved in the air
    public float GroundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
    public float SlowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
    public float StickToGroundHelperDistance = 0.5f; // stops the character
    public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f),
                new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

    [Range(0.25f, 3)] public float MinTimeForFall = 0.75f;
    public float HoverDamping = 35;
    public float GroundDrag = 2.5f;
    public float AirDrag = 1f;
    public float MaxGroundForce = 2000;
    public float MaxHeightForHover = 1.25f;
    /// Maximum difference between force angle and engine angle. When set to 0, engine applies force along its local Y
    /// axis (meaning that when hover is angled, hover force would move it sideways). If MaxHoverAngleDrift is nonzero, force
    /// direction can be rotated to match world Y axis even when engine itself is angled.
    public float MaxHoverAngleDrift = 55;
    /// ExponentHeightForce in height-to-force relationship. When 1, force falls linearly with height, when 2 - quadratically etc
    [Range(1, 3)] public float ExponentHeightForce = 1.5f;
    public float ClimbDistance = 1f;
    public float ClimbHeightMulti = 1.85f;
    public Vector3 ClimbStep = new Vector3(0, 0.25f, 0);
    public float ClimbTime = 0.25f;
    [Range(0, 1)] public float DodgeMovePercent = 0.65f;
    public float LookAheadGroundingDistance = 0.5f;
}
