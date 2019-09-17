using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class DataDescriptionComponent : IComponent {
        public string Text;
        public DataDescriptionComponent(string text){
            Text = text;
        }
        public DataDescriptionComponent(){}

        public DataDescriptionComponent(SerializationInfo info, StreamingContext context) {
            Text = info.GetValue(nameof(Text), Text);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Text), Text);
        }

    }

    public struct DataDescriptionUpdating : IEntityMessage {
        public DataDescriptionComponent Data;

        public DataDescriptionUpdating(DataDescriptionComponent data) {
            Data = data;
        }
    }
}
