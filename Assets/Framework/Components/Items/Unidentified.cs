using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class Unidentified : IComponent {
        public int Owner { get; set; }
        public float Difficulty { get; }
        public int Skill { get; }

        public Unidentified(int owner, float difficulty, int skill) {
            Owner = owner;
            Difficulty = difficulty;
            Skill = skill;
        }
    }
}
