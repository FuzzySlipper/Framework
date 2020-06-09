using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    
    public class AbilityAttack : ActionHandler {

        public DiceValue Damage;
        [ValueDropdown("DamageTypeList")] public string Defense;

        private ValueDropdownList<string> DamageTypeList() {
            return Defenses.GetDropdownList();
        }
        
        public override void SetupEntity(Entity entity) {
            entity.Get<StatsContainer>().Add(new DiceStat(entity, Stats.Damage, Damage));
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            var damageStat = cmd.Action.Stats.Get<DiceStat>(Stats.Damage);
            if (damageStat == null) {
                return;
            }
            var total = cmd.HitResult.Result == CollisionResult.CriticalHit ? damageStat.GetMax() : damageStat.Value;
            prepareDamage.Entries.Add(new DamageEntry(total, Defense, Stats.Health, cmd.Action.GetName()));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}
