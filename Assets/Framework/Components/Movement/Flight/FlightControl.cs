using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightControl : ComponentBase {
        public Mode CurrentMode;
        public FlightControlConfig Config;

        public float Pitch;
        public float Yaw;
        public float Roll;
        public float StrafeHorizontal;
        public float StrafeVertical;
        public float Thrust;
        public float Boost;
        public float Speed;
        
        public Vector3 GotoPos;
        public bool EnginesActivated;

        public FlightControl(FlightControlConfig config) {
            Config = config;
            EnginesActivated = true;
            CurrentMode = config.DefaultMode;
            ClearValues();
        }
        
        public void ClearValues() {
            Yaw = Pitch = Roll = Thrust = StrafeHorizontal = StrafeVertical = 0;
        }

        public void SetMode() {
            CurrentMode = Config.DefaultMode;
            ClearValues();
        }

        public void SetMode(Mode mode) {
            CurrentMode = mode;
            ClearValues();
        }

        public enum Mode {
            Hovering,
            Disabled,
            FakeFlying,
            Flying,
            KinematicFlying
        }
    }
}
