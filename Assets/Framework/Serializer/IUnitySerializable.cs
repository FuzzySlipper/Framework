using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public interface IUnitySerializable : ISerializable {

        /// The type of component stored.
        System.Type Type { get; set; }

        /// Called after an object is deserialized and constructed to it's base `type`.
        void ApplyProperties(object obj);

        /// Called before serialization, any properties stoed in the returned dictionary
        /// will be saved and re-applied in ApplyProperties.
        Dictionary<string, object> PopulateSerializableDictionary();
    }
}
