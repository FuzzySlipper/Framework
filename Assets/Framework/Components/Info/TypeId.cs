using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public struct TypeId : IComponent {
        public string Id { get; }

        public TypeId(string id) {
            Id = id;
        }

        public TypeId(SerializationInfo info, StreamingContext context) {
            Id = info.GetValue(nameof(Id), "");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Id), Id);
        }

        public static implicit operator string(TypeId id) {
            return id.Id;
        }
    }
}
