using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

namespace PixelComrades {
    // a version of System.Type that can be serialized
    //From https://stackoverflow.com/questions/12306/can-i-serialize-a-c-sharp-type-object
    
    [DataContract]
    public class SerializableType : ISerializable {
        public Type TargetType;
        
        [DataMember]
        string TypeString {
            get {
                if (TargetType == null)
                    return null;
                return TargetType.FullName;
            }
            set {
                if (value == null)
                    TargetType = null;
                else {
                    TargetType = Type.GetType(value);
                }
            }
        }

        // constructors
        public SerializableType() {
            TargetType = null;
        }

        public SerializableType(Type t) {
            TargetType = t;
        }

        public SerializableType(SerializationInfo info, StreamingContext context) {
            TargetType = Type.GetType(info.GetValue(nameof(TargetType), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(TargetType), TypeString);
        }

        // allow SerializableType to implicitly be converted to and from System.Type
        static public implicit operator Type(SerializableType stype) {
            return stype.TargetType;
        }

        static public implicit operator SerializableType(Type t) {
            return new SerializableType(t);
        }

        // overload the == and != operators
        public static bool operator ==(SerializableType a, SerializableType b) {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null)) {
                return false;
            }

            // Return true if the fields match:
            return a.TargetType == b.TargetType;
        }

        public static bool operator !=(SerializableType a, SerializableType b) {
            return !(a == b);
        }
        // we don't need to overload operators between SerializableType and System.Type because we already enabled them to implicitly convert

        public override int GetHashCode() {
            return TargetType.GetHashCode();
        }

        // overload the .Equals method
        public override bool Equals(System.Object obj) {
            // If parameter is null return false.
            if (obj == null) {
                return false;
            }

            // If parameter cannot be cast to SerializableType return false.
            SerializableType p = obj as SerializableType;
            if ((System.Object) p == null) {
                return false;
            }

            // Return true if the fields match:
            return (TargetType == p.TargetType);
        }

        public bool Equals(SerializableType p) {
            // If parameter is null return false:
            if ((object) p == null) {
                return false;
            }

            // Return true if the fields match:
            return (TargetType == p.TargetType);
        }
    }
}
