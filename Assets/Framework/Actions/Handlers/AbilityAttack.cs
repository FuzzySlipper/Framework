using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    [System.Serializable]
    
    public class AbilityAttack : ActionHandler {

        [SerializeField] private DiceValue _damage = new DiceValue();
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")] 
        private string _bonusStat = Attributes.Insight;
        [SerializeField, DropdownList(typeof(DamageTypes), "GetValues")] 
        private string _damageType = DamageTypes.Physical;

        public override void SetupEntity(Entity entity) {
            entity.Get<StatsContainer>().Add(new DiceStat(entity, Stat.Damage, _damage));
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var total = RulesSystem.CalculateDamageTotal(cmd.Action.Stats.Get<DiceStat>(Stat.Damage), cmd, _bonusStat);
            if (total <= 0) {
                return;
            }
            var prepareDamage = new PrepareDamageEvent(ae.Origin, ae.Target, cmd.Action, cmd.HitResult);
            prepareDamage.Entries.Add(new DamageEntry(total, _damageType, Stat.Health, cmd.Action.GetName()));
            World.Get<RulesSystem>().Post(prepareDamage);
        }
    }
}
