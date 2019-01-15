using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades.DungeonCrawler {
    public class Weapon : IComponent, IReceive<EquipmentChanged>, IReceive<DataDescriptionAdded> {
        private int _owner = -1;
        public int Owner {
            get { return _owner; }
            set {_owner = value; }
        }

        private CommandSequence _attack;
        private CommandsContainer _current;

        public CommandSequence Attack { get => _attack; }

        public Weapon(CommandSequence attack) {
            _attack = attack;
        }

        public void Handle(EquipmentChanged arg) {
            if (_current != null) {
                _current.Remove(_attack);
                _current.GetEntity().Get<DefaultCommand>(d => d.Alternative = null);
                _current = null;
            }
            if (arg.Owner != null) {
                _current = arg.Owner.Get<CommandsContainer>();
            }
            if (_current != null) {
                _current.Add(_attack);
                _current.GetEntity().Get<DefaultCommand>(d => d.Alternative = _attack);
            }
        }

        public void Handle(DataDescriptionAdded arg) {
            var entity = this.GetEntity();
            FastString.Instance.Clear();
            FastString.Instance.AppendNewLine(entity.Stats.Get<RangeStat>(Stats.Power)?.ToLabelString());
            FastString.Instance.AppendNewLine(entity.Stats.Get(Stats.ToHit)?.ToLabelString());
            FastString.Instance.AppendNewLine(entity.Stats.Get(Stats.CriticalHit)?.ToLabelString());
            FastString.Instance.AppendNewLine(entity.Stats.Get(Stats.CriticalMulti)?.ToLabelString());
            if (_attack.Costs.SafeAccess(0) is RecoveryCost cost) {
                FastString.Instance.AppendBoldLabel("Recovery", cost.Percent * 100);
                FastString.Instance.AppendNewLine("%");
            }
            FastString.Instance.AppendBoldLabelNewLine("Two Handed", entity.Tags.Contain(EntityTags.TwoHandedWeapon).ToString());
            var targeting = entity.Get<CommandTargeting>();
            FastString.Instance.AppendBoldLabelNewLine("Range", targeting.Range);
            var dmgImpact = entity.Get<ActionImpacts>()?[0] as DamageImpact;
            FastString.Instance.AppendBoldLabelNewLine("Damage Type", GameData.DamageTypes.GetNameAt(dmgImpact != null ? dmgImpact.DamageType : "DamageTypes.Physical"));
            var spawn = entity.Get<ActionSpawnComponent>().Data;
            if (spawn != null) {
                FastString.Instance.AppendBoldLabelNewLine("Projectile", spawn.TryGetValue(DatabaseFields.Name, spawn.ID));
            }
            arg.Data.Text += FastString.Instance.ToString();
        }
    }
}
