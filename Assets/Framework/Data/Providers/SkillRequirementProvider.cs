using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SkillRequirementProvider : IDataFactory<SkillRequirement> {

        public void AddComponent(Entity entity, DataEntry data) {
            entity.Add(new SkillRequirement(data.GetValue<string>("Skill"), ParseUtilities.TryParseEnum(data.TryGetValue("Required", "None"), 0)));
        }
    }
}
