using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class AddStatModAction : AddModAction {
        [SerializeField] private DiceValue _amount = new DiceValue();
        [SerializeField] private string _amountStat = Stats.Power;
        [SerializeField, DropdownList(typeof(Attributes), "GetValues")] private string _bonusStat = Attributes.Insight;
        [SerializeField, DropdownList(typeof(Stats), "GetValues")] private string _stat = Stats.Armor;

        public override void SetupEntity(Entity entity) {
            entity.Get<StatsContainer>().Add(new DiceStat(entity, _amountStat, _amount));
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var total = RulesSystem.CalculateDamageTotal(cmd.Action.Stats.Get<DiceStat>(_amountStat), cmd, _bonusStat);
            if (total <= 0) {
                return;
            }
            var mod = new TimedStatMod(ae, Length, _stat, total);
            RulesSystem.Get.Post(new ApplyModEvent(ae, mod));
        }
    }
}
