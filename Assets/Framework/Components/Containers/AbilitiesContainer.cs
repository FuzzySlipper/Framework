using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class AbilitiesContainer : IComponent, IEntityContainer {

        public event System.Action OnRefreshItemList;

        private EntityContainer _container;

        public AbilitiesContainer(int limit = -1) {
            _container = new EntityContainer(limit);
        }

        public AbilitiesContainer(SerializationInfo info, StreamingContext context) {
            _container = info.GetValue(nameof(_container), _container);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_container), _container);
        }

        public Entity this[int index] { get { return _container[index]; } }
        public int Count { get { return _container.Count; } }
        public Entity Owner { get { return this.GetEntity(); } }
        public bool IsFull { get { return false; } }

        public bool Contains(Entity item) {
            for (int i = 0; i < Count; i++) {
                if (this[i] == item) {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string id) {
            for (int i = 0; i < Count; i++) {
                if (this[i].Get<TypeId>().Id == id) {
                    return true;
                }
            }
            return false;
        }

        public void ContainerSystemSet(Entity item, int index) {
            _container.Add(item);
        }
        
        public int ContainerSystemAdd(Entity item) {
            var abilityData = item.Get<AbilityData>();
            AddAbility(abilityData, item);
            return _container.Add(item);
        }

        public bool Remove(Entity entity) {
            if (!_container.Contains(entity)) {
                return false;
            }
            var msg = new ContainerStatusChanged(null, entity);
            entity.Post(msg);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
            return true;
        }

        public void Clear() {
            _container.Clear();
            var msg = new ContainerStatusChanged(this, null);
            this.GetEntity().Post(msg);
            OnRefreshItemList.SafeInvoke();
        }

        private void AddAbility(AbilityData abilityData, Entity item) {
            var itemStats = item.Get<StatsContainer>();
            var ownerStats = this.GetEntity().Get<StatsContainer>();
            ownerStats.Get(abilityData.Template.Skill).AddDerivedStat(RpgSettings.SkillToHitBonus, itemStats.Get(Stats.ToHit));
            var source = abilityData.Template.Source;
            ownerStats.Get(source.GetPowerFromSource()).AddDerivedStat(1, itemStats.Get(Stats.Power));
            ownerStats.Get(source.GetToHitFromSource()).AddDerivedStat(1, itemStats.Get(Stats.ToHit));
            ownerStats.Get(source.GetCritFromSource()).AddDerivedStat(1, itemStats.Get(Stats.CriticalHit));
        }
    }
}