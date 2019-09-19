using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {

    [System.Serializable]
	public sealed class SpellsContainer : IComponent {
        public SpellsContainer() {}

        public SpellsContainer(SerializationInfo info, StreamingContext context) {
            _knownSpells = info.GetValue(nameof(_knownSpells), _knownSpells);
            _spellLevels = info.GetValue(nameof(_spellLevels), _spellLevels);
            _spellSchools = info.GetValue(nameof(_spellSchools), _spellSchools);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_knownSpells), _knownSpells);
            info.AddValue(nameof(_spellLevels), _spellLevels);
            info.AddValue(nameof(_spellSchools), _spellSchools);
        }

        private Dictionary<string, SpellData> _knownSpells = new Dictionary<string, SpellData>();
        private Dictionary<int, List<SpellData>> _spellLevels = new Dictionary<int, List<SpellData>>();
        private Dictionary<int, List<SpellData>> _spellSchools = new Dictionary<int, List<SpellData>>();

        public Dictionary<string, SpellData> KnownSpells { get { return _knownSpells; } }
        public Dictionary<int, List<SpellData>> SpellLevels { get { return _spellLevels; } }
        public Entity Owner { get { return this.GetEntity(); } }
        
        public void AddToSpellLists(SpellData item) {
            GetLevelList(item.Template.Level).Add(item);
            GetSkillList(item.Template.Skill).Add(item);
        }

        public List<SpellData> GetLevelList(int level) {
            if (!_spellLevels.TryGetValue(level, out var list)) {
                list = new List<SpellData>();
                _spellLevels.Add(level, list);
            }
            return list;
        }

        public List<SpellData> GetSkillList(int skill) {
            if (!_spellSchools.TryGetValue(skill, out var list)) {
                list = new List<SpellData>();
                _spellSchools.Add(skill, list);
            }
            return list;
        }

        public List<SpellData> GetSkillList(string skill) {
            return GetSkillList(GameData.Skills.GetIndex(skill));
        }

        public int GetCount(int skill) {
            if (!_spellSchools.TryGetValue((int)skill, out var list)) {
                return 0;
            }
            return list.Count;
        }

        public bool HasSpell(AbilityConfig template) {
            return _knownSpells.ContainsKey(template.ID);
        }

    }
}