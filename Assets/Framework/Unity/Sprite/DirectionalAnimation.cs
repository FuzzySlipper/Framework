using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.Serialization;

namespace PixelComrades {

    
    public class DirectionalAnimation : SpriteAnimation, IDirectionalAnimation {
        
        [SerializeField, FormerlySerializedAs("Sprites")] private Sprite[] _sprites;
        [SerializeField, FormerlySerializedAs("Colliders")]private SavedSpriteCollider[] _colliders;
        public List<DirectionalFrames> DirectionalFrames = new List<DirectionalFrames>();
        
        public override int LengthSprites { get { return _sprites.Length; } }
        public override Sprite[] Sprites { get => _sprites; set => _sprites = value; }
        public override SavedSpriteCollider[] Colliders { get => _colliders; set => _colliders = value; }

        
        public override Sprite GetSprite(int spriteIdx) {
            return Sprites[spriteIdx];
        }

        public override SavedSpriteCollider GetSpriteCollider(int spriteIdx) {
            if (Colliders == null || Colliders.Length == 0) {
                return null;
            }
            return Colliders[spriteIdx];
        }
        
        public Sprite GetSprite(DirectionsEight facing, int spriteIdx) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                //var idx = frames[spriteIdx];
                var idx = frames.SafeAccess(spriteIdx);
                return Sprites[Mathf.Clamp(idx, 0, Sprites.Length - 1)];
            }
            return GetSprite(spriteIdx);
        }

        public SavedSpriteCollider GetSpriteCollider(DirectionsEight facing, int spriteIdx) {
            var frames = GetFacingIndices(facing);
            if (frames != null) {
                var idx = frames[spriteIdx];
                return Colliders[Mathf.Clamp(idx, 0, Colliders.Length - 1)];
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