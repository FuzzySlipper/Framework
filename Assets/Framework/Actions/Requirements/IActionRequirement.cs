using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IActionRequirement {
        string Description(BaseActionTemplate template, CharacterTemplate character);
        bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target);
        bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target);
    }
}
