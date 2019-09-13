using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class HoverEngine : IComponent {

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

        public HoverEngine(SerializationInfo info, StreamingContext context) {
            Config = ItemPool.LoadAsset<HoverEngineConfig>(info.GetValue(nameof(Config), ""));
            Drag = Config.DriveForce / Config.MaxForwardSpeed;
            RefreshPid();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Config), ItemPool.GetAssetLocation(Config));
        }
    }
}
