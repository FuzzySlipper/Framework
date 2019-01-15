using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class DataDescriptionComponent : IComponent {
        public int Owner { get; set; }
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
    }

    public struct DataDescriptionAdded : IEntityMessage {
        public DataDescriptionComponent Data;

        public DataDescriptionAdded(DataDescriptionComponent data) {
            Data = data;
        }
    }
}
