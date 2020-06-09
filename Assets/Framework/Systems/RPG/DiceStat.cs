using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelComrades {
    public class DiceStat : BaseStat {
        
        public int DiceRolls;
        public int DiceSides;
        public int MultiplyResult = 1;

        public DiceStat(int entity, string label, DiceValue diceValue) : base(entity, label, diceValue.Bonus) {
            DiceRolls = diceValue.DiceRolls;
            DiceSides = diceValue.DiceSides;
        }

        public DiceStat(int entity, string label, float baseValue, int diceRolls, int diceSides) : base(entity, label, baseValue) {
            DiceRolls = diceRolls;
            DiceSides = diceSides;
        }

        private DiceStat(int entity, string label) : base(entity, label, 0) { }

        public override float Value {
            get {
                return MultiplyResult * (base.Value + RulesSystem.RollDice(DiceSides, DiceRolls));
            }
        }

        public float GetMax() {
            return MultiplyResult * (RulesSystem.GetMaxDice(DiceSides, DiceRolls) + BaseValue + ModTotal);
        }

        public override string ToString() {
            return string.Format("{0}: {1}d{2}+{3:F0}", Label, DiceRolls, DiceSides, Value);
        }

        public override string ToLabelString() {
            return Label.ToBoldLabel(string.Format("{0}d{1}+{2:F0}", DiceRolls, DiceSides, Value));
        }

        private static Regex _whitespacePattern = new Regex(@"\s+");

        public static DiceStat Parse(int entity, string label, string expression) {
            string cleanExpression = _whitespacePattern.Replace(expression.ToLower(), "");
            cleanExpression = cleanExpression.Replace("+-", "-");

            var parseValues = new ParseValues().Init();
            var dice = new DiceStat(entity, label);

            for (int i = 0; i < cleanExpression.Length; ++i) {
                char c = cleanExpression[i];
                if (char.IsDigit(c)) {
                    parseValues.Constant += c;
                }
                else if (c == '*') {
                    parseValues.Multiply *= int.Parse(parseValues.Constant);
                    parseValues.Constant = "";
                }
                else if (c == 'd') {
                    if (parseValues.Constant == "") {
                        parseValues.Constant = "1";
                    }
                    parseValues.DiceRolls = int.Parse(parseValues.Constant);
                    parseValues.Constant = "";
                }
                // else if (c == 'k') {
                //     string chooseAccum = "";
                //     while (i + 1 < cleanExpression.Length && char.IsDigit(cleanExpression[i + 1])) {
                //         chooseAccum += cleanExpression[i + 1];
                //         ++i;
                //     }
                //     parseValues.Choose = int.Parse(chooseAccum);
                // }
                else if (c == '+') {
                    Append(dice, parseValues);
                    parseValues = new ParseValues().Init();
                }
                else if (c == '-') {
                    Append(dice, parseValues);
                    parseValues = new ParseValues().Init();
                    parseValues.Multiply = -1;
                }
                else {
                    throw new ArgumentException("Invalid character in dice expression", expression);
                }
            }
            Append(dice, parseValues);

            return dice;
        }

        private static void Append(DiceStat dice, ParseValues parseValues) {
            int constant = int.Parse(parseValues.Constant);
            if (parseValues.DiceRolls == 0) {
                dice.ChangeBase(parseValues.Multiply * constant);
            }
            else {
                dice.DiceSides = constant;
                dice.DiceRolls = parseValues.DiceRolls;
                dice.MultiplyResult = parseValues.Multiply;
            }
        }

        private struct ParseValues {
            public string Constant;
            public int Multiply;
            public int DiceRolls;

            public ParseValues Init()
            {
                Multiply = 1;
                Constant = "";
                return this;
            }
        }
    }
}
