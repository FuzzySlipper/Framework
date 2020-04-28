using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelComrades {
    public class AbilityFactory : ScriptableDatabase<AbilityFactory> {

        [SerializeField] private List<AbilityConfig> _allAbilities = new List<AbilityConfig>();
        
        private static Dictionary<string, AbilityConfig> _abilities = new Dictionary<string, AbilityConfig>();
        public override IEnumerable<UnityEngine.Object> AllObjects { get { return _allAbilities; } }
        public override System.Type DbType { get { return typeof(AbilityConfig); } }

        public override void AddObject(Object obj) {
            var item = obj as AbilityConfig;
            if (item == null || _allAbilities.Contains(item)) {
                return;
            }
            _allAbilities.Add(item);
        }
        
        public override T GetObject<T>(string id) {
            return _abilities.TryGetValue(id, out var target) ? target as T : null;
        } 

        public override string GetId<T>(T obj) {
            return obj is AbilityConfig config ? config.ID : "";
        }

        public override void CleanObjectList() {
            for (int i = _allAbilities.Count - 1; i >= 0; i--) {
                if (_allAbilities[i] == null) {
                    _allAbilities.RemoveAt(i);
                }
            }
        }

        private static void Init() {
            GameData.AddInit(Init);
            _abilities.Clear();
            for (int i = 0; i < Main._allAbilities.Count; i++) {
                var ability = Main._allAbilities[i];
                if (ability == null) {
                    continue;
                }
                _abilities.AddOrUpdate(ability.ID, ability);
            }
        }

        public static Entity GetRandom() {
            return BuildAbility(Main._allAbilities.RandomElement());
        }

        public static Entity Get(string id, bool ignoreCost = false) {
            return BuildEntity(id, ignoreCost);
        }

        public static AbilityConfig GetConfig(string id) {
            if (_abilities.Count == 0) {
                Init();
            }
            return _abilities.TryGetValue(id, out var data) ? data : null;
        }

        public static Dictionary<string, AbilityConfig> GetDict() {
            if (_abilities.Count == 0) {
                Init();
            }
            return _abilities;
        }

        private static Entity BuildEntity(string id, bool ignoreVitalCost = false) {
            if (_abilities.Count == 0) {
                Init();
            }
            var data = GetConfig(id);
            if (data == null) {
                Debug.LogFormat("{0} didn't load Ability", id);
                return null;
            }
            return BuildAbility(data);
        }

        public static Entity BuildAbility(AbilityConfig config) {
            var entity = Entity.New(config.Name);
            entity.Add(new TypeId(config.ID));
            entity.Add(new LabelComponent(entity.Name));
            entity.Add(new DescriptionComponent(config.Description));
            entity.Add(new StatsContainer());
            if (config.Icon.IsLoaded) {
                entity.Add(new IconComponent(config.Icon.LoadedAsset, ""));
            }
            else {
                config.Icon.LoadAsset(
                    handle => {
                        entity.Add(new IconComponent(handle, ""));
                    });
            }
            entity.Add(new InventoryItem(1, 0, ItemRarity.Special));
            entity.Add(new StatusUpdateComponent());
            config.AddComponents(entity);
            // entity.Add(new DataDescriptionComponent(config.DataDescription));
            return entity;
        }
    }

    
}
