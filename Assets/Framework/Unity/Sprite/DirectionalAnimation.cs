using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

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
            if (DirectionalFrames == null || DirectionalFrames.Count == 0) {
                return null;
            }
            return DirectionalFrames[0].Frames[frame];
        }

        [Button]
        public void DoubleRearFrames() {
            for (int d = 0; d < DirectionalFrames.Count; d++) {
                var frames = DirectionalFrames[d].Frames;
                switch (DirectionalFrames[d].Side) {
                    case DirectionsEight.Rear:
                    case DirectionsEight.RearLeft:
                    case DirectionsEight.Left:
                    case DirectionsEight.RearRight:
                    case DirectionsEight.Right:
                        for (int f = 1; f < frames.Length; f++) {
                            if (f % 2 != 0) {
                                frames[f] = frames[f - 1];
                            }
                        }
                        break;
                }
            }
        }
    }
}