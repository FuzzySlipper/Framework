using UnityEngine;
using System.Collections;

public class HeadBobSettings : ScriptableObject {
    [Range(.1f, 2f)]
    public float BobHeightSpeedMultiplier = .35f;
    [Range(.1f, 2f)]
    public float BobStrideSpeedLengthen = .35f;
    [Range(1f, 3f)]
    public float HeadBobFrequency = 1.5f;
    [Range(.1f, 2f)]
    public float HeadBobHeight = .35f;
    [Range(.01f, .1f)]
    public float HeadBobSideMovement = .075f;
    [Range(.1f, 2f)]
    public float HeadBobSwayAngle = .5f;
    [Range(.1f, 5f)]
    public float JumpLandMove = 2f;
    [Range(10f, 100f)]
    public float JumpLandTilt = 35f;
    [Range(.1f, 2f)]
    public float SpringDampen = .77f;
    [Range(.1f, 4f)]
    public float SpringElastic = 1.25f;

    [Range(.1f, 2f)]
    public float WeaponSpeedMultiplier = .77f;
    [Range(0.01f, 0.5f)]
    public float MaxMoveAmountX = 0.05f;
    [Range(0.01f, 0.5f)]
    public float MaxMoveAmountY = 0.05f;
    [Range(1f, 15f)]
    public float RotateAmount = 5f;
    [Range(1f, 10f)]
    public float RotationSpeed = 2f;
    [Range(.05f, 5f)]
    public float WeaponBobSpeed = 5f;
}
