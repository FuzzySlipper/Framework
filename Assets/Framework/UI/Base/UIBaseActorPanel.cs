using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class UIBaseActorPanel : UIBasicMenu {

        protected PlayerCharacterTemplate Actor;

        public virtual void SetActor(PlayerCharacterTemplate actor) {
            Actor = actor;
        }
    }
}
