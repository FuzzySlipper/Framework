using UnityEngine;
using System.Collections;

namespace PixelComrades {

    public enum OrientationMethod {
        TowardsCamera,
        TowardsMovement,
    }

    public class FpControllerSettings : ScriptableObject {

        public float MaxStableMoveSpeed = 10f;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;
        public bool OrientTowardsGravity;

        public float AirAccelerationSpeed = 5f;
        public bool AllowJumpingWhenSliding = true;
        public float Drag = 0.1f;
        public float JumpSpeed = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

        public float CrouchMoveMulti = 0.75f;
        public float SprintMulti = 1.5f;
        public float DodgeForce = 15f;
        public float DodgeTime = 0.5f;
        public float DodgeUpForce = 15f;
        public float AnchoringDuration = 0.25f;
        public float ClimbingSpeed = 4f;
        public float JumpClimbForce = 1.5f;
        public float SwimmingSpeed = 4f;
        public float SwimmingMovementSharpness = 3;
        public float FlyingSpeed = 4f;
    }
}