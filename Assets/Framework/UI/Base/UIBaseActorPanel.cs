using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UIBaseActorPanel : UIBasicMenu {

        protected CharacterNode Actor;

        public virtual void SetActor(CharacterNode actor) {
            Actor = actor;
        }
    }
}
