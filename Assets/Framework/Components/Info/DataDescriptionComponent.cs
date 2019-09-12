using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class DataDescriptionComponent : IComponent {
        private string _text;
        public string Text {
            get {
                if (OnDataDescription != null) {
                    return OnDataDescription(this);
                }
                return _text;
            }
            set { _text = value; }
        }

        public System.Func<IComponent, string> OnDataDescription;

        public DataDescriptionComponent(string text){
            _text = text;
        }
        public DataDescriptionComponent(){}

        public DataDescriptionComponent(SerializationInfo info, StreamingContext context) {
            _text = info.GetValue(nameof(_text), _text);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_text), _text);
        }

    }

    public struct DataDescriptionAdded : IEntityMessage {
        public DataDescriptionComponent Data;

        public DataDescriptionAdded(DataDescriptionComponent data) {
            Data = data;
        }
    }
}
