using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class Unidentified : IComponent {
        public float Difficulty { get; }
        public int Skill { get; }

        public Unidentified(float difficulty, int skill) {
            Difficulty = difficulty;
            Skill = skill;
        }

        public Unidentified(SerializationInfo info, StreamingContext context) {
            Difficulty = info.GetValue(nameof(Difficulty), Difficulty);
            Skill = info.GetValue(nameof(Skill), Skill);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Difficulty), Difficulty);
            info.AddValue(nameof(Skill), Skill);
        }
    }
}
