using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public sealed class WeaponMeleeAttack : ActionHandler {

        private const string Label = "WeaponAttack";

        [ValueDropdown("DamageTypeList")] public string DamageType = Defenses.Physical;

        private ValueDropdownList<string> DamageTypeList() {
            return Defenses.GetDropdownList();
        }        
        public override void SetupEntity(Entity entity) { }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            var damageStat = cmd.Owner.Stats.Get<DiceStat>(Stats.WeaponAttackDamage);
            if (damageStat == null) {
                return;
            }
            var total = cmd.HitResult.Result == CollisionResult.CriticalHit ? damageStat.GetMax() : damageStat.Value;
            prepareDamage.Entries.Add(new DamageEntry(total, DamageType, Stats.Health, "Weapon Attack"));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}
