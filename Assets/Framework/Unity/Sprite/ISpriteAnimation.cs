using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ISpriteAnimation {
        float FrameTime { get; }
        int LengthFrames { get; }
        AnimationFrame[] Frames { get; set; }
        float FramesPerSecond { get; set; }
        bool Looping { get; }
        Texture2D NormalMap { get; }
        Texture2D EmissiveMap { get; }
        Sprite GetSprite(int spriteIdx);
        SavedSpriteCollider GetSpriteCollider(int spriteIdx);
    }
    public interface IDirectionalAnimation : ISpriteAnimation {
        Sprite GetSprite(DirectionsEight facing, int spriteIdx);
        SavedSpriteCollider GetSpriteCollider(DirectionsEight facing, int spriteIdx);
        DirectionalFrames GetFacing(DirectionsEight side);
    }
}
