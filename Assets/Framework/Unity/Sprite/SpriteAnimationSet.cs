using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimationSet : SpriteAnimation {
        
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private SavedSpriteCollider[] _colliders;
        public override int LengthSprites { get { return _sprites.Length; } }
        public override Sprite[] Sprites { get => _sprites; set => _sprites = value; }
        public override SavedSpriteCollider[] Colliders { get => _colliders; set => _colliders = value; }
    }
}
