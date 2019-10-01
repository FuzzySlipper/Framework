using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public interface IFirstPersonController {
        float SoundVolume { get; }
        AudioSource Audio { get; }
        float VelocityPercent { get; }
        Vector3 Velocity { get; }
        Transform Tr { get; }
        bool Grounded { get; }
        FPMovementAction CurrentMovement { get; }
        SurfaceDetector.SurfaceData CurrentSurface { get; set; }
    }

    public enum FPMovementAction {
        None,
        Moving,
        Running,
        Jumping,
        Climbing,
        Dodging
    }
}
