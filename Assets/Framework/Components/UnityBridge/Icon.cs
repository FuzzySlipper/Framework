using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class IconComponent : IComponent {
        public int Owner { get; set; }
        public Sprite Sprite;

        public IconComponent(Sprite sprite) {
            Sprite = sprite;
        }

        public static implicit operator Sprite(IconComponent component) {
            if (component == null) {
                return null;
            }
            return component.Sprite;
        }
    }
}
