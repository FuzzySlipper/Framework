using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class HoverEngineConfig : ScriptableObject {
        [Header("Hover Settings")] 
        public float MaxGroundDist = 15f;
        public float HoverForce = 100f;
        public LayerMask GroundLayer = new LayerMask();
        public PIDController HoverPid = new PIDController();
        public float HoverHeight = 10f;
        [Header("Physics Settings")] 
        public float MaxForwardSpeed = 100f;
        public float BoostSpeed = 15f;
        public float HoverGravity = 8f;
        public float FallGravity = 16f;
        [Header("Drive Settings")] 
        public float DriveForce = 20f;
        public float SlowingVelFactor = .99f; //(e.g., a value of .99 means the ship loses 1% velocity when not thrusting)
        public float BrakingVelFactor = 0.95f;
        public float JumpForce = 3000;
        public float TurnForce = 3f;
        public float PitchForce = 3f;
        public float StrafeForce = 1500f;
        public float UpOrientationSpeed = 0.075f;
        public float OrientSpeed = 10f;
    }
}
