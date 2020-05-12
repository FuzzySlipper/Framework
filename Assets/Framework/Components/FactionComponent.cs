using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public struct FactionComponent : IComponent {

        private int _value;

        public int Value { get { return _value; } } 

        public FactionComponent(int value) : this() {
            _value = value;
        }

        public static implicit operator int(FactionComponent reference) {
            return reference.Value;
        }

        public FactionComponent(SerializationInfo info, StreamingContext context) {
            _value = info.GetValue(nameof(_value), 0);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_value), _value);
        }
    }
}
