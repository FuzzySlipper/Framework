using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DirectionalSprite : ScriptableObject {

        public Texture2D NormalMap;
        public Texture2D EmissiveMap;
        public List<DirectionalFrame> Frames = new List<DirectionalFrame>();
        public float Offset = 0;

        public Sprite GetFacingSprite(DirectionsEight side) {
            if (Frames == null || Frames.Count == 0) {
                return null;
            }
            for (int i = 0; i < Frames.Count; i++) {
                if (Frames[i].Side == side) {
                    return Frames[i].Frame;
                }
            }
            return Frames[0].Frame;
        }
    }
}