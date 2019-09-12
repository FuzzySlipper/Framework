using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class FlightControl : IComponent {
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

        public FlightControl(SerializationInfo info, StreamingContext context) {
            Config = ItemPool.LoadAsset<FlightControlConfig>(info.GetValue(nameof(Config), ""));
            CurrentMode = info.GetValue(nameof(CurrentMode), CurrentMode);
            EnginesActivated = info.GetValue(nameof(EnginesActivated), EnginesActivated);
            ClearValues();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Config), ItemPool.GetAssetLocation(Config));
            info.AddValue(nameof(CurrentMode), CurrentMode);
            info.AddValue(nameof(EnginesActivated), EnginesActivated);
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
