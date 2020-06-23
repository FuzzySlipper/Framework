using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class CostUse : CommandCost, ISerializable {

        public override void ProcessCost(ActionTemplate action, CharacterTemplate owner) {
            owner.Get<LimitedUses>()?.Use();
        }

        public CostUse() {}

        public CostUse(SerializationInfo info, StreamingContext context) {}

        public void GetObjectData(SerializationInfo info, StreamingContext context) {}

        public override bool CanAct(ActionTemplate action, CharacterTemplate owner) {
            var uses = action.Get<LimitedUses>();
            if (uses == null || uses.Current == 0) {
                owner.Post(new StatusUpdate( owner,"No more uses", Color.yellow));
                return false;
            }
            return true;
        }
    }
}
