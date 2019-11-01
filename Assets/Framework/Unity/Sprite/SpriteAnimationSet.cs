using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteAnimationSet : ScriptableObject {
        
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private SavedSpriteCollider[] _colliders;
        [SerializeField] private List<ChildSpriteAnimation> _children = new List<ChildSpriteAnimation>(); 
        public Sprite[] Sprites { get => _sprites; set => _sprites = value; }
        public SavedSpriteCollider[] Colliders { get => _colliders; set => _colliders = value; }

        public void AddChild(ChildSpriteAnimation child) {
            child.Set = this;
            _children.Add(child);
        }
    }
}
