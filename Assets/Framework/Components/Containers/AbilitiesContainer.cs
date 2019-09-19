using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AbilitiesContainer : IComponent, IEntityContainer {

        public event System.Action OnRefreshItemList;

        private EntityContainer _entityContainer;

        public AbilitiesContainer(int limit = -1) {
            _entityContainer = new EntityContainer(limit);
        }

        public AbilitiesContainer(SerializationInfo info, StreamingContext context) {
            _entityContainer = info.GetValue(nameof(_entityContainer), _entityContainer);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_entityContainer), _entityContainer);
        }

        public Entity this[int index] { get { return _entityContainer[index]; } }
        public int Count { get { return _entityContainer.Count; } }
        public Entity Owner { get { return this.GetEntity(); } }
        public bool CanAdd(Entity entity) {
            if (!_entityContainer.CanAdd(entity) || (!entity.HasComponent<AbilityData>() && !entity.HasComponent<SpellData>())) {
                return false;
            }
            return true;
        }

        public bool Add(Entity item) {
            if (!_entityContainer.Add(item)) {
                return false;
            }
            item.ParentId = this.GetEntity();
            var abilityData = item.Get<AbilityData>();
            if (abilityData == null) {
                var spellData = item.Get<SpellData>();
                if (spellData != null) {
                    AddSpell(spellData, item);
                }
            }
            else {
                AddAbility(abilityData, item);                
            }
            item.Get<InventoryItem>().SetContainer(this);
            var msg = new ContainerStatusChanged(this, item);
            item.Post(msg);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        public bool Remove(Entity entity) {
            if (!_entityContainer.Add(entity)) {
                return false;
            }
            entity.Get<InventoryItem>()?.SetContainer(null);
            entity.ParentId = -1;
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        public void Clear() {
            _entityContainer.Clear();
            var msg = new ContainerStatusChanged(this, null);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
        }

        private void AddAbility(AbilityData abilityData, Entity item) {
            var itemStats = item.Get<StatsContainer>();
            var ownerStats = this.GetEntity().Get<StatsContainer>();
            ownerStats.Get(GameData.Skills.GetID(abilityData.Template.Skill)).AddDerivedStat(RpgSettings.SkillToHitBonus, itemStats.Get(Stats.ToHit));
            var source = abilityData.Template.Source;
            ownerStats.Get(source.GetPowerFromSource()).AddDerivedStat(1, itemStats.Get(Stats.Power));
            ownerStats.Get(source.GetToHitFromSource()).AddDerivedStat(1, itemStats.Get(Stats.ToHit));
            ownerStats.Get(source.GetCritFromSource()).AddDerivedStat(1, itemStats.Get(Stats.CriticalHit));
        }

        private void AddSpell(SpellData abilityData, Entity item) {
            var itemStats = item.Get<StatsContainer>();
            var ownerStats = this.GetEntity().Get<StatsContainer>();
            var source = abilityData.Template.Source;
            ownerStats.Get(GameData.Skills.GetID(abilityData.Template.Skill)).AddDerivedStat(RpgSettings.SkillToHitBonus, itemStats.Get(Stats.ToHit));
            ownerStats.Get(source.GetPowerFromSource()).AddDerivedStat(1, itemStats.Get(Stats.Power));
            ownerStats.Get(source.GetToHitFromSource()).AddDerivedStat(1, itemStats.Get(Stats.ToHit));
            ownerStats.Get(source.GetCritFromSource()).AddDerivedStat(1, itemStats.Get(Stats.CriticalHit));
        }
    }
}