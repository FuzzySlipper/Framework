using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class DescriptionComponent : IComponent {
        public string Text;

        public DescriptionComponent(string text){
            Text = text;
        }

        public DescriptionComponent(SerializationInfo info, StreamingContext context) {
            Text = info.GetValue(nameof(Text), "");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Text), Text);
        }

    }
}
