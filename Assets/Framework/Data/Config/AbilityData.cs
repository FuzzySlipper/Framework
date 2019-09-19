using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AbilityData : IComponent{
        public AbilityConfig Template { get; }

        public AbilityData(AbilityConfig template) {
            Template = template;
        }

        public AbilityData(SerializationInfo info, StreamingContext context) {
            Template = AbilityFactory.GetTemplate(info.GetValue(nameof(Template), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Template), Template.ID);
        }
    }
}
