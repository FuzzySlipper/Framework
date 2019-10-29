using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Sirenix.OdinInspector;
using System.Linq;

namespace PixelComrades {
    public class DirectionalAnimation : SpriteAnimation {

        public List<DirectionalFrames> DirectionalFrames = new List<DirectionalFrames>();
        public override int LengthSprites { get { return Sprites.Length; } }
        public override Sprite GetSprite(int frame) {
            return Sprites[frame];
        }

        public override SavedSpriteCollider GetSpriteCollider(int frame) {
            if (Colliders == null || Colliders.Length == 0) {
                return null;
            }
            return Colliders[frame];
        }
        
        public Sprite GetSprite(DirectionsEight facing, int frame) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[frame];
                return Sprites[Mathf.Clamp(idx, 0, Sprites.Length - 1)];
            }
            return GetSprite(frame);
        }

        public SavedSpriteCollider GetSpriteCollider(DirectionsEight facing, int frame) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[frame];
                return Colliders[Mathf.Clamp(idx, 0, Colliders.Length - 1)];
            }
            return null;
        }

        private int[] GetFacingIndices(DirectionsEight side) {
            for (int i = 0; i < DirectionalFrames.Count; i++) {
                if (DirectionalFrames[i].Side == side) {
                    return DirectionalFrames[i].FrameIndices;
                }
            }
            return null;
        }

        public DirectionalFrames GetFacing(DirectionsEight side) {
            for (int i = 0; i < DirectionalFrames.Count; i++) {
                if (DirectionalFrames[i].Side == side) {
                    return DirectionalFrames[i];
                }
            }
            return null;
        }
    }
}