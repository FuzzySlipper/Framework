using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [System.Serializable]
    public sealed class NormalAttack : ActionHandler {

        [SerializeField] private DiceValue _damage = new DiceValue();
        [SerializeField, DropdownList(typeof(DamageTypes), "GetValues")]
        private string _damageType = DamageTypes.Physical;

        public override void SetupEntity(Entity entity) {
            entity.Get<StatsContainer>().Add(new DiceStat(entity, Stat.Damage, _damage));
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var total = RulesSystem.CalculateDamageTotal(cmd.Owner.Stats.Get<DiceStat>(Stat.AttackDamage), cmd, ae.Origin.GetAttackDamageBonusStatName());
            if (total <= 0) {
                return;
            }
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            prepareDamage.Entries.Add(new DamageEntry(total, _damageType, Stat.Health, "Attack"));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}
