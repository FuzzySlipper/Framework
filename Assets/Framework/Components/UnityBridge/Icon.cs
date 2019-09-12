using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class IconComponent : IComponent {
        public Sprite Sprite;
        public string IconLocation { get; }

        public IconComponent(Sprite sprite, string iconLocation) {
            Sprite = sprite;
            IconLocation = iconLocation;
        }

        public IconComponent(string iconLocation) {
            Sprite = ItemPool.LoadAsset<Sprite>(iconLocation);
            IconLocation = iconLocation;
        }

        public IconComponent(string dir, string icon) {
            IconLocation = ItemPool.GetCombinedLocator(dir, icon);
            Sprite = ItemPool.LoadAsset<Sprite>(IconLocation);
        }

        public IconComponent(){}

        public static implicit operator Sprite(IconComponent component) {
            if (component == null) {
                return null;
            }
            return component.Sprite;
        }

        public IconComponent(SerializationInfo info, StreamingContext context) {
            IconLocation = info.GetValue(nameof(IconLocation), IconLocation);
            Sprite = ItemPool.LoadAsset<Sprite>(IconLocation);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(IconLocation), IconLocation);
        }
    }
}
