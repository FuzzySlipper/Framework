using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class HoverEngine : ComponentBase {

        public bool IsOnGround;
        public float Height;
        public bool Jumping;
        public float Drag;
        public Vector3 GroundNormal = Vector3.up;
        public PIDController HoverPid = new PIDController();
        public HoverEngineConfig Config { get; }

        public HoverEngine(HoverEngineConfig config) {
            Config = config;
            Drag = Config.DriveForce / Config.MaxForwardSpeed;
            RefreshPid();
        }

        public void RefreshPid() {
            HoverPid.P = Config.HoverPid.P;
            HoverPid.I = Config.HoverPid.I;
            HoverPid.D = Config.HoverPid.D;
        }
    }
}
