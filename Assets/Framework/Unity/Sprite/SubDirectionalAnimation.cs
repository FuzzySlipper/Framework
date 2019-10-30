using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SubDirectionalAnimation : SpriteAnimation, IDirectionalAnimation {

        public SpriteAnimationSet Set;
        public List<DirectionalFrames> DirectionalFrames = new List<DirectionalFrames>();
        public override int LengthSprites { get { return DirectionalFrames[0].FrameIndices.Length; } }
        public override Sprite[] Sprites { get { return Set.Sprites; } set { } }
        public override SavedSpriteCollider[] Colliders { get { return Set.Colliders; } set { } }

        public override Sprite GetSprite(int frame) {
            return Set.Sprites[DirectionalFrames[0].FrameIndices[frame]];
        }

        public override SavedSpriteCollider GetSpriteCollider(int frame) {
            if (Set.Colliders == null || Set.Colliders.Length == 0) {
                return null;
            }
            return Set.Colliders[DirectionalFrames[0].FrameIndices[frame]];
        }

        public Sprite GetSprite(DirectionsEight facing, int frame) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[frame];
                return Set.Sprites[Mathf.Clamp(idx, 0, Set.Sprites.Length - 1)];
            }
            return Set.GetSprite(frame);
        }

        public SavedSpriteCollider GetSpriteCollider(DirectionsEight facing, int frame) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[frame];
                return Set.Colliders[Mathf.Clamp(idx, 0, Set.Colliders.Length - 1)];
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

        private int[] GetFacingIndices(DirectionsEight side) {
            for (int i = 0; i < DirectionalFrames.Count; i++) {
                if (DirectionalFrames[i].Side == side) {
                    return DirectionalFrames[i].FrameIndices;
                }
            }
            return null;
        }
    }
}
