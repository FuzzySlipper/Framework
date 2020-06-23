using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades;

namespace PixelComrades {
    public class UIBaseActorPanel : UIBasicMenu {

        protected PlayerCharacterTemplate Actor;

        public virtual void SetActor(PlayerCharacterTemplate actor) {
            Actor = actor;
        }
    }
}
