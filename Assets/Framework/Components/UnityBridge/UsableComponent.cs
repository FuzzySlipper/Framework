using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class UsableComponent : IComponent {
        public int Owner { get; set; }
        public Func<IComponent, bool> OnUsableDel;
        public Func<IComponent, bool> OnSecondaryDel;
        public System.Object LastRequester { get; private set; }

        public bool TryUse(System.Object requester) {
            LastRequester = requester;
            if (OnUsableDel != null) {
                return OnUsableDel(this);
            }
            return false;
        }

        public bool TrySecondary(System.Object requester) {
            LastRequester = requester;
            if (OnSecondaryDel != null) {
                return OnSecondaryDel(this);
            }
            return false;
        }
    }
}
