using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    
    [Priority(Priority.Highest)]
    public class BlockDamage : IComponent, IReceiveRef<DamageEvent> {

        public int Owner { get; set; }

        public List<Func<DamageEvent, bool>> Dels = new List<Func<DamageEvent, bool>>();

        public void Handle(ref DamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            for (int i = 0; i < Dels.Count; i++) {
                if (Dels[i](arg)) {
                    arg.Amount = 0;
                    break;
                }
            }
        }

    }
}
