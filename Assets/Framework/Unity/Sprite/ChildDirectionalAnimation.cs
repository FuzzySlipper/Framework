using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ChildDirectionalAnimation : ChildSpriteAnimation, IDirectionalAnimation {

        public List<DirectionalFrames> DirectionalFrames = new List<DirectionalFrames>();
        public override int LengthSprites { get { return DirectionalFrames[0].FrameIndices.Length; } }
        public override Sprite[] Sprites { get { return Set.Sprites; } set { } }
        public override SavedSpriteCollider[] Colliders { get { return Set.Colliders; } set { } }

        public override Sprite GetSprite(int spriteIdx) {
            return Set.Sprites[DirectionalFrames[0].FrameIndices[spriteIdx]];
        }

        public override SavedSpriteCollider GetSpriteCollider(int spriteIdx) {
            if (Set.Colliders == null || Set.Colliders.Length == 0) {
                return null;
            }
            return Set.Colliders[DirectionalFrames[0].FrameIndices[spriteIdx]];
        }

        public Sprite GetSprite(DirectionsEight facing, int spriteIdx) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[spriteIdx];
                return Set.Sprites[Mathf.Clamp(idx, 0, Set.Sprites.Length - 1)];
            }
            return Set.Sprites[spriteIdx];
        }

        public SavedSpriteCollider GetSpriteCollider(DirectionsEight facing, int spriteIdx) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[spriteIdx];
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
