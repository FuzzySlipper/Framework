using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SimpleAnimation : SpriteAnimation {

        public override Sprite GetSprite(int frame) {
            return Sprites[Mathf.Clamp(frame, 0, Sprites.Length - 1)];
        }

        public override SavedSpriteCollider GetSpriteCollider(int frame) {
            if (Colliders == null || Colliders.Length == 0) {
                return null;
            }
            return Colliders[Mathf.Clamp(frame, 0, Colliders.Length - 1)];
        }
    }
}
