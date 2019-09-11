using UnityEngine;
using System.Collections;
namespace PixelComrades {
    public class SimpleAnimation : SpriteAnimation {

        public Sprite[] Sprites;

        public override Sprite GetSpriteFrame(int frame) {
            return Sprites[Mathf.Clamp(frame, 0, Sprites.Length - 1)];
        }
    }
}
