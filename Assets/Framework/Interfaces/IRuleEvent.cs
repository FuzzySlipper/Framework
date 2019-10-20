using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IRuleEvent {
        ActionTemplate Action { get; }
        CharacterTemplate Origin { get; }
        CharacterTemplate Target { get; }
    }
}
