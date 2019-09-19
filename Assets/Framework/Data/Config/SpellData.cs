using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SpellData : IComponent {
        public AbilityConfig Template { get; }

        public SpellData(AbilityConfig template) {
            Template = template;
        }

        public SpellData(SerializationInfo info, StreamingContext context) {
            var spellId = info.GetValue(nameof(Template), "");
            Template = SpellFactory.GetTemplate(spellId);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Template), Template.ID);
        }
    }
}
