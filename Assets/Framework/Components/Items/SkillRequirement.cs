using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class SkillRequirement : IComponent {
        public string Skill { get; }
        public int Required { get; }

        public SkillRequirement(string skill, int required) {
            Skill = skill;
            Required = required;
        }

        public SkillRequirement(SerializationInfo info, StreamingContext context) {
            Skill = info.GetValue(nameof(Skill), Skill);
            Required = info.GetValue(nameof(Required), Required);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Skill), Skill);
            info.AddValue(nameof(Required), Required);
        }
    }
}
