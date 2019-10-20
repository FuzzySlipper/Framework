using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpellFactory {
        

        private static List<AbilityConfig>[] _spellLevels = new List<AbilityConfig>[9];
        private static List<AbilityConfig>[] _spellMinLevels = new List<AbilityConfig>[9];
        private static List<AbilityConfig>[] _spellMaxLevels = new List<AbilityConfig>[9];

        private static bool _setup = false;

        private static void Init() {
            GameData.AddInit(Init);
            for (int i = 0; i < _spellLevels.Length; i++) {
                _spellLevels[i] = new List<AbilityConfig>();
                _spellMinLevels[i] = new List<AbilityConfig>();
                _spellMaxLevels[i] = new List<AbilityConfig>();
            }
            var dict = AbilityFactory.GetDict();
            foreach (var dataEntry in dict) {
                var spell = dataEntry.Value;
                int lvl = spell.Level;
                if (!_spellMaxLevels.HasIndex(lvl)) {
                    Debug.LogErrorFormat("Spell {0} has invalid lvl {1}", spell.Name, lvl);
                    return;
                }
                if (lvl > 1) {
                    for (int m = lvl - 1; m >= 0; m--) {
                        _spellMaxLevels[m].Add(spell);
                    }
                }
                else {
                    _spellMaxLevels[lvl].Add(spell);
                }
                for (int m = lvl; m < _spellMinLevels.Length; m++) {
                    _spellMinLevels[m].Add(spell);
                }
                _spellLevels[lvl].Add(spell);
            }
            _setup = true;
        }

        public int TotalLevels {  get {
            if (!_setup) {
                Init();
            }
            return _spellLevels.Length;
        } }

        public static AbilityConfig GetTemplate(string templateName) {
            if (!_setup) {
                Init();
            }
            return AbilityFactory.GetConfig(templateName);
        }

        public static SpellData GetRandomExact(int level) {
            if (!_setup) {
                Init();
            }
            return AbilityFactory.BuildAbility(_spellLevels[level].RandomElement(), false).Get<SpellData>();
        }

        public static AbilityConfig GetRandomExactTemplate(int level) {
            if (!_setup) {
                Init();
            }
            return _spellLevels[level].RandomElement();
        }

        public static SpellData GetRandomMin(int level) {
            if (!_setup) {
                Init();
            }
            return AbilityFactory.BuildAbility(_spellMinLevels[level].RandomElement(), false).Get<SpellData>();
        }

        public static SpellData GetRandomMinNoDuplicate(int level, string skill, CharacterTemplate actor) {
            if (!_setup) {
                Init();
            }
            var spellsContainer = actor.Entity.Get<SpellsContainer>();
            int chk = 0;
            while (chk < 10000) {
                chk++;
                var template = _spellMinLevels[level].RandomElement();
                if (template.Skill == skill && !spellsContainer.HasSpell(template)) {
                    return AbilityFactory.BuildAbility(template, false).Get<SpellData>();
                }
            }
            return null;
        }

        public static List<AbilityConfig> GetUnknownSpellsList(int level, string skill, CharacterTemplate actor) {
            List<AbilityConfig> spells = new List<AbilityConfig>();
            FillUnknownSpellsList(level, skill, actor, ref spells);
            return spells;
        }

        public static void FillUnknownSpellsList(int level, string skill, CharacterTemplate actor, ref List<AbilityConfig> spells) {
            if (!_setup) {
                Init();
            }
            var spellsContainer = actor.Entity.Get<SpellsContainer>();
            var list = _spellMaxLevels[level];
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Skill == skill && !spellsContainer.HasSpell(list[i])) {
                    spells.Add(list[i]);
                }
            }
        }
    }
}
