using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SimpleAnimation : SpriteAnimation {

        public Sprite[] Sprites;
        public SavedSpriteCollider[] Colliders;

        public override Sprite GetSpriteFrame(int frame) {
            return Sprites[Mathf.Clamp(frame, 0, Sprites.Length - 1)];
        }

        public override SavedSpriteCollider GetSpriteCollider(int frame) {
            return Colliders[Mathf.Clamp(frame, 0, Colliders.Length - 1)];
        }
    }
}
