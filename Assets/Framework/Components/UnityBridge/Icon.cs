using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class IconComponent : IComponent {
        public Sprite Sprite;
        public string IconLocation { get; }

        public IconComponent(Sprite sprite, string iconLocation) {
            Sprite = sprite;
            IconLocation = iconLocation;
        }

        public IconComponent(string iconLocation) {
            IconLocation = iconLocation;
            Sprite = ItemPool.LoadAssetOld<Sprite>(IconLocation);
        }

        public IconComponent(string dir, string icon) {
            IconLocation = ItemPool.GetCombinedLocator(dir, icon);
            Sprite = ItemPool.LoadAssetOld<Sprite>(IconLocation);
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
            ItemPool.LoadAsset<Sprite>(IconLocation, a => Sprite = a);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(IconLocation), IconLocation);
        }
    }
}
