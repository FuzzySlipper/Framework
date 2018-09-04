using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DirectionalAnimation : SpriteAnimation {

        public List<DirectionalFrames> DirectionalFrames = new List<DirectionalFrames>();

        public Sprite GetSpriteFrame(DirectionsEight facing, int frame) {
            var frames = GetFacingSprites(facing);
            if (frames != null) {
                return frames[Mathf.Clamp(frame, 0, frames.Length - 1)];
            }
            return GetSpriteFrame(frame);
        }

        private Sprite[] GetFacingSprites(DirectionsEight side) {
            for (int i = 0; i < DirectionalFrames.Count; i++) {
                if (DirectionalFrames[i].Side == side) {
                    return DirectionalFrames[i].Frames;
                }
            }
            return null;
        }

        public override Sprite GetSpriteFrame(int frame) {
            return DirectionalFrames[0].Frames[frame];
        }

    }
}