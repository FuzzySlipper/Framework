using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class SerializedV3 : ISerializable {

        public Vector3? Value;

        public SerializedV3(Vector3? value) {
            Value = value;
        }

        public SerializedV3(){}
        
        public SerializedV3(SerializationInfo info, StreamingContext context) {
            var v3String = info.GetValue(nameof(Value), "");
            if (ParseUtilities.TryParse(v3String, out Vector3 v3)) {
                Value = v3;
            }
            else {
                Value = null;
            }
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Value), Value != null ? ParseUtilities.EncodeV3(Value.Value) : "");
        }
    }
}
