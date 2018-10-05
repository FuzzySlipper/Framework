using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FloatingTextStatusComponent : IComponent, IReceive<StatusUpdate> {
        public int Owner { get; set; }

        public void Handle(StatusUpdate arg) {
            var tr = this.Get<TransformComponent>().Tr;
            if (tr == null) {
                return;
            }
            FloatingText.Spawn(arg.Update, 3f, tr.position);
        }
    }
}
