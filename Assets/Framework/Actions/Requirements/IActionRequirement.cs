using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IActionRequirement {
        string Description(ActionTemplate template, CharacterTemplate character);
        bool CanTarget(ActionTemplate template, CharacterTemplate character, CharacterTemplate target);
        bool CanEffect(ActionTemplate template, CharacterTemplate character, CharacterTemplate target);
    }
}
