using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PixelComrades {
    public sealed class SerializedQuaternion : ISerializable {

        public Quaternion? Value;

        public SerializedQuaternion(Quaternion? value) {
            Value = value;
        }

        public SerializedQuaternion(){}
        
        public SerializedQuaternion(SerializationInfo info, StreamingContext context) {
            var qtString = info.GetValue(nameof(Value), "");
            if (ParseUtilities.TryParse(qtString, out Quaternion v3)) {
                Value = v3;
            }
            else {
                Value = null;
            }
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Value), Value != null ? ParseUtilities.EncodeEulerQuaternion(Value.Value) : "");
        }
    }
}
