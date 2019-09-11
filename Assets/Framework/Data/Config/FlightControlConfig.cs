using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightControlConfig : ScriptableObject {
        [Header("Input")] 
        public float PitchSensitivity = 0.2f;
        public float YawSensitivity = 0.2f;
        [Header("AutoLevel")]
        public FlightControl.Mode DefaultMode = FlightControl.Mode.Flying;
        public bool AutoLevel = false;
        public float AutoRollSpeed = -0.75f;
        public bool AltOrient = true;
        public float HorizonOrientAngle = -1;
        [Header("Banking")] 
        public float AngleOfRoll = 15f;
        public float RollSpeed = 5f;
        [Header("PlayerInput")] 
        public bool LinkYawAndRoll = false;
        public float YawRollRatio = 1;
        public bool MouseVerticalInverted = false;
        public float MousePitchSensitivity = 1;
        public float MouseYawSensitivity = 1;
        public float MouseRollSensitivity = 1;
        public float MouseDeadRadius = 0;
        public bool UseMouse = true;
        public bool UseDirectControl = true;
    }
}
