using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class LevelingSystem : SystemBase, IReceiveGlobal<LevelUpEvent> {
        public LevelingSystem(){}
        public void HandleGlobal(LevelUpEvent arg) {
            for (int l = 0; l < arg.Character.Stats.Count; l++) {
                var leveledStat = arg.Character.Stats[l] as LeveledDiceStat;
                if (leveledStat == null) {
                    continue;
                }
                while (true) {
                    bool foundValue = false;
                    for (int i = 0; i < leveledStat.LeveledValues.Count; i++) {
                        var levelBonus = leveledStat.LeveledValues[i];
                        if (levelBonus.Level <= arg.Level) {
                            leveledStat.AddToBase(levelBonus.BonusValue);
                            leveledStat.DiceSides += levelBonus.BonusDice;
                            leveledStat.DiceRolls += levelBonus.BonusRolls;
                            leveledStat.LeveledValues.RemoveAt(i);
                            foundValue = true;
                            break;
                        }
                    }
                    if (!foundValue) {
                        break;
                    }
                }
            }
        }
    }

    public struct LevelUpEvent : IEntityMessage {
        public CharacterTemplate Character;
        public int Level;
        public string Class;

        public LevelUpEvent(CharacterTemplate character, int level, string className = "") {
            Character = character;
            Level = level;
            Class = className;
        }
    }
}
