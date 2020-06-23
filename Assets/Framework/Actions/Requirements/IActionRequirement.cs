using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IActionRequirement {
<<<<<<< HEAD
        string Description(BaseActionTemplate template, CharacterTemplate character);
        bool CanTarget(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target);
        bool CanEffect(BaseActionTemplate template, CharacterTemplate character, CharacterTemplate target);
=======
        string Description(ActionTemplate template, CharacterTemplate character);
        bool CanTarget(ActionTemplate template, CharacterTemplate character, CharacterTemplate target);
        bool CanEffect(ActionTemplate template, CharacterTemplate character, CharacterTemplate target);
>>>>>>> FirstPersonAction
    }
}
