using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public sealed class NormalAttack : ActionHandler {

        [SerializeField] private bool _unarmed = false;
        [SerializeField] private DiceValue _damage = new DiceValue();
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")]
        private string _bonusStat = Attributes.Insight;
        [SerializeField, DropdownList(typeof(DamageTypes), "GetValues")]
        private string _damageType = DamageTypes.Physical;

        public override void SetupEntity(Entity entity) {
            entity.Get<StatsContainer>().Add(new DiceStat(entity, Stats.Damage, _damage));
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var total = RulesSystem.CalculateDamageTotal(cmd.Owner.Stats.Get<DiceStat>(_unarmed ? Stats.WeaponAttackDamage : Stats.UnarmedAttackDamage), cmd, _bonusStat);
            if (total <= 0) {
                return;
            }
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            prepareDamage.Entries.Add(new DamageEntry(total, _damageType, Stats.Health, "Weapon Attack"));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}
