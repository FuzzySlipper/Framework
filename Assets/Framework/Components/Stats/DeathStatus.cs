using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.Lowest)]
    public class DeathStatus : IComponent, IReceive<DamageEvent> {
        public int Owner { get; set; }

        public bool IsDead = false;

        public void Handle(DamageEvent arg) {
            if (this.GetEntity().Stats.HealthStat?.Current > 0.05f) {
                return;
            }
            IsDead = true;
            this.GetEntity().Tags.Add(EntityTags.IsDead);
            this.GetEntity().Post(new DeathEvent(arg.Origin, arg.Target));
        }

        public void Raise() {
            IsDead = false;
            this.GetEntity().Tags.Remove(EntityTags.IsDead);
            this.GetEntity().Post(new RaiseDead(this.GetEntity()));
        }
    }
}
