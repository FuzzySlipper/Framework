using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
	[System.Serializable]
	public struct AmountDescriptionComponent : IComponent {
        public string Text;

        public AmountDescriptionComponent(string text = "") {
            Text = text;
        }
        
        public AmountDescriptionComponent(SerializationInfo info, StreamingContext context) {
            Text = info.GetValue(nameof(Text), "");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Text), Text);
        }
    }
}
