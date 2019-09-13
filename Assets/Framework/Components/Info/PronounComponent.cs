using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class PronounComponent : IComponent {
        public PlayerPronouns Pronoun;

        public PronounComponent(PlayerPronouns pronoun) {
            Pronoun = pronoun;
        }

        public static implicit operator PlayerPronouns(PronounComponent comp) {
            return comp.Pronoun;
        }

        public PronounComponent(SerializationInfo info, StreamingContext context) {
            Pronoun = info.GetValue(nameof(Pronoun), Pronoun);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Pronoun), Pronoun);
        }
    }
}
