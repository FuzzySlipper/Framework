using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class ChildSimpleAnimation : ChildSpriteAnimation {

        public int[] Indices = new int[0];
        public override int LengthSprites { get { return Indices.Length; } }
        public override Sprite[] Sprites { get { return Set.Sprites; } set {} }
        public override SavedSpriteCollider[] Colliders { get { return Set.Colliders; } set { } }

        public override Sprite GetSprite(int frame) {
            return Set.Sprites[Indices[frame]];
        }

        public override SavedSpriteCollider GetSpriteCollider(int frame) {
            if (Set.Colliders == null || Set.Colliders.Length == 0) {
                return null;
            }
            return Set.Colliders[Indices[frame]];
        }
    }
}
