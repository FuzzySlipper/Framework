using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class LabelComponent : IComponent {
        public string Text;

        public LabelComponent(string text){
            Text = text;
        }

        public LabelComponent(SerializationInfo info, StreamingContext context) {
            Text = info.GetValue(nameof(Text), "");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Text), Text);
        }
    }
}
