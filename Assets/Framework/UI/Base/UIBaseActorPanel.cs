using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UIBaseActorPanel : UIBasicMenu {

        protected CharacterTemplate Actor;

        public virtual void SetActor(CharacterTemplate actor) {
            Actor = actor;
        }
    }
}
