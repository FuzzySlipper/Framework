using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class PronounComponent : IComponent {
        public int Owner { get; set; }
        public PlayerPronouns Pronoun;

        public PronounComponent(PlayerPronouns pronoun) {
            Pronoun = pronoun;
        }

        public static implicit operator PlayerPronouns(PronounComponent comp) {
            return comp.Pronoun;
        }
    }
}
