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
        Sprite GetSprite(int frame);
        SavedSpriteCollider GetSpriteCollider(int frame);
    }
    public interface IDirectionalAnimation : ISpriteAnimation {
        Sprite GetSprite(DirectionsEight facing, int frame);
        SavedSpriteCollider GetSpriteCollider(DirectionsEight facing, int frame);
        DirectionalFrames GetFacing(DirectionsEight side);
    }
}
