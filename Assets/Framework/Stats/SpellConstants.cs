using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PixelComrades {
    
    public enum MagicActionWords {
        Summon = 0,
        Control = 1,
        Sunder = 2,
        Invoke = 3,
        Reveal = 4,
    }
    
    public enum Domains {
        [Description("Physical")]
        Physical = 0, //Chaos/Primal/Maelstrom
        [Description("Frost")]
        Frost = 1, //Order/Ice/Law
        [Description("Fire")]
        Fire = 2, //Freedom/Fire
        [Description("Spirit")]
        Spirit = 3, //or Air
        [Description("Mind")]
        Mind = 4,
        None = 10,
    }

    public enum MagicModifierWords {
        None = 0,
        Area = 1,
        Self = 2,
        Amplify = 3,
        Move = 4,
    }
    
    public static class SpellIncantations {
        
        public static string[] Actions = new[] {
            "eol", // make 
            "vaoan", // truth
            "napta", //napta
            "vinu", // invoke
            "fifalz" // weed out
        };

        public static string[] Domains = new[] {
            "valasa", // end
            "caba", // govern
            "zilna", //itself - self
            "malprg", //fire
            "omaxa", //know
        };

        public static string[] Mods = new[] {
            "",
            "gea", // we are 
            "ol", //i myself / make
            "uml", //add
            "zac", //could be zna, zacam, zacar, zacare all movoe you
        };

        public static string Description(this MagicActionWords word) {
            return Actions[(int) word];
        }
        public static string Description(this Domains word) {
            return Domains[(int)word];
        }
        public static string Description(this MagicModifierWords word) {
            return Mods[(int)word];
        }

        public static bool IsActionWord(string word, out MagicActionWords actionWord) {
            var index = FindPlaceInArray(Actions, word);
            actionWord = (MagicActionWords) index;
            return index >= 0;
        }

        public static bool IsDomainWord(string word, out Domains actionWord) {
            var index = FindPlaceInArray(Domains, word);
            actionWord = (Domains) index;
            return index >= 0;
        }

        public static MagicModifierWords GetMod(string[] words, int startIndex) {
            for (int i = startIndex; i < words.Length; i++) {
                var index = FindPlaceInArray(Mods, words[i]);
                if (index >= 0) {
                    return ((MagicModifierWords)index);
                }
            }
            return MagicModifierWords.None;
        }

        private static int FindPlaceInArray(string[] array, string word) {
            for (int i = 0; i < array.Length; i++) {
                if (array[i] == word) {
                    return i;
                }
            }
            return -1;
        }
    }

}