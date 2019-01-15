using System;
using UnityEngine;

namespace PixelComrades {
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ConditionalAttribute : PropertyAttribute {
        public enum Comparison {
            Equals,
            NotEqualTo,
            GreaterThan,
            LessThan,
            EqualsOrGreaterThan,
            EqualsOrLessThan
        }

        public enum IsAnyOf {
            Yes,
            No
        }

        public enum IsBetween {
            ExclusiveYes,
            ExclusiveNo,
            InclusiveYes,
            InclusiveNo
        }

        public enum Logical {
            AND,
            OR
        }

        public Where[] conditions;
        public bool shouldBeShown = true;

        public ConditionalAttribute(string[] propertyNames, int[] comparisons, object[] values, int[] logicals) {
            conditions = new Where[propertyNames.Length];

            for (var i = 0; i < conditions.Length; i++) {
                conditions[i].propertyName = propertyNames[i];

                if (i < comparisons.Length) {
                    conditions[i].comparison = (Comparison) comparisons[i];
                }
                else {
                    conditions[i].comparison = Comparison.Equals; //Set it to == if no value was given
                }

                if (i < values.Length) {
                    conditions[i].value = values[i];
                }
                else {
                    conditions[i].value = values[values.Length - 1]; //Set it to the last value if no value was given
                }

                if (i < logicals.Length) {
                    conditions[i].logical = (Logical) logicals[i];
                }
                else {
                    conditions[i].logical = Logical.AND; //Set it to && if no value was given
                }
            }
        }

        public ConditionalAttribute(string propertyName, params object[] values)
            : this(propertyName, IsAnyOf.Yes, values) {
        }

        public ConditionalAttribute(string propertyName, IsAnyOf ctype, params object[] values) {
            conditions = new Where[values.Length];

            switch (ctype) {
                case IsAnyOf.Yes:
                    for (var i = 0; i < conditions.Length; i++) {
                        conditions[i] = new Where(propertyName, Comparison.Equals, values[i], Logical.OR);
                    }

                    break;
                case IsAnyOf.No:
                    for (var i = 0; i < conditions.Length; i++) {
                        conditions[i] = new Where(propertyName, Comparison.NotEqualTo, values[i], Logical.AND);
                    }

                    break;
            }
        }

        public ConditionalAttribute(string propertyName, IsBetween ctype, object value1, object value2) {
            conditions = new Where[2];

            conditions[0] = new Where(propertyName, Comparison.Equals, value1, Logical.AND);
            conditions[1] = new Where(propertyName, Comparison.Equals, value2, Logical.AND);

            switch (ctype) {
                case IsBetween.ExclusiveYes:
                    conditions[0].comparison = Comparison.GreaterThan;
                    conditions[1].comparison = Comparison.LessThan;
                    break;
                case IsBetween.ExclusiveNo:
                    conditions[0].comparison = Comparison.LessThan;
                    conditions[1].comparison = Comparison.GreaterThan;
                    break;
                case IsBetween.InclusiveYes:
                    conditions[0].comparison = Comparison.EqualsOrGreaterThan;
                    conditions[1].comparison = Comparison.EqualsOrLessThan;
                    break;
                case IsBetween.InclusiveNo:
                    conditions[0].comparison = Comparison.EqualsOrLessThan;
                    conditions[1].comparison = Comparison.EqualsOrGreaterThan;
                    break;
            }
        }

        public ConditionalAttribute(string propertyName, object value) {
            conditions = new Where[1];
            conditions[0] = new Where(propertyName, Comparison.Equals, value);
        }

        public ConditionalAttribute(string propertyName, object value, int logical) {
            conditions = new Where[1];
            conditions[0] = new Where(propertyName, Comparison.Equals, value, (Logical) logical);
        }

        public ConditionalAttribute() {
        }

        public struct Where {
            public string propertyName;
            public Comparison comparison;
            public object value;
            public Logical logical;

            public Where(string propertyName, Comparison comparison, object value, Logical logical = Logical.AND) {
                this.propertyName = propertyName;
                this.comparison = comparison;
                this.value = value;
                this.logical = logical;
            }
        }
    }
}