using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Sirenix.OdinInspector;

namespace PixelComrades {

    public class AssociatedValue : Attribute {
        public int Value = 0;

        public AssociatedValue(int value) {
            this.Value = value;
        }
    }

    public abstract class GenericEnum<T, U> where T : GenericEnum<T, U>, new() {
        private static readonly List<string> _names;
        private static readonly List<string> _descriptions;
        private static readonly List<int> _associatedValues;
        private static readonly List<U> _values;
        private static bool _allowInstanceExceptions;
        private static string _typeName;
        protected int _index;

        public abstract U Parse(string value, U defValue);

        private static T _static = new T();

        public static U Parse(IList<string> lines, ref int parseIndex, U defValue) {
            if (!lines.HasIndex(parseIndex)) {
                parseIndex++;
                return defValue;
            }
            var strVal = lines[parseIndex];
            parseIndex++;
            for (int i = 0; i < Count; i++) {
                if (strVal == GetNameAt(i) || strVal == GetIdAt(i)) {
                    return ValueAt(i);
                }
            }
            return _static.Parse(strVal, defValue);
        }

        public static U TryParse(string line, U defValue) {
            return _static.Parse(line, defValue);
        }

        static GenericEnum() {
            Type t = typeof(T);
            Type u = typeof(U);
            if (t == u) {
                throw new InvalidOperationException(string.Format("{0} and its underlying type cannot be the same", t.Name));
            }
            _typeName = t.Name + ".";
            BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
            FieldInfo[] fia = t.GetFields(bf);
            _names = new List<string>();
            _descriptions = new List<string>();
            _associatedValues = new List<int>();
            _easyList = new ValueDropdownList<U>();
            _values = new List<U>();
            for (int i = 0; i < fia.Length; i++) {
                if (fia[i].FieldType == u && (fia[i].IsLiteral || fia[i].IsInitOnly)) {
                    _names.Add(fia[i].Name);
                    _values.Add((U) fia[i].GetValue(null));
                    _easyList.Add(new ValueDropdownItem<U>(_names.LastElement(), _values.LastElement()));
                    DescriptionAttribute description = Attribute.GetCustomAttribute(fia[i], typeof(DescriptionAttribute)) as DescriptionAttribute;
                    _descriptions.Add(description != null ? description.Description : fia[i].Name);
                    AssociatedValue defValue = Attribute.GetCustomAttribute(fia[i], typeof(AssociatedValue)) as AssociatedValue;
                    _associatedValues.Add(defValue != null ? defValue.Value : 0);
                }
            }
            if (_names.Count == 0) {
                throw new InvalidOperationException(string.Format("{0} has no suitable fields", t.Name));
            }
        }

        public static bool AllowInstanceExceptions { get { return _allowInstanceExceptions; } set { _allowInstanceExceptions = value; } }
        public static Type UnderlyingType { get { return typeof(U); } }
        public static int Count { get { return _names.Count; } }
        public int Index {
            get { return _index; }
            set {
                if (value < 0 || value >= Count) {
                    if (_allowInstanceExceptions) {
                        throw new ArgumentException(string.Format("Index must be between 0 and {0}", Count - 1));
                    }
                    return;
                }
                _index = value;
            }
        }
        public string Name {
            get { return _names[_index]; }
            set {
                int index = _names.IndexOf(value);
                if (index == -1) {
                    if (_allowInstanceExceptions) {
                        throw new ArgumentException(string.Format("'{0}' is not a defined name of {1}", value, typeof(T).Name));
                    }
                    return;
                }
                _index = index;
            }
        }
        public string Description {
            get { return _descriptions[_index]; }
            set {
                int index = _descriptions.IndexOf(value);
                if (index == -1) {
                    if (_allowInstanceExceptions) {
                        throw new ArgumentException(string.Format("'{0}' is not a defined name of {1}", value, typeof(T).Name));
                    }
                    return;
                }
                _index = index;
            }
        }
        public U Value {
            get { return _values[_index]; }
            set {
                int index = _values.IndexOf(value);
                if (index == -1) {
                    if (_allowInstanceExceptions) {
                        throw new ArgumentException(string.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
                    }
                    return;
                }
                _index = index;
            }
        }

        public static T ByIndex(int index) {
            if (index < 0 || index >= Count) {
                if (_allowInstanceExceptions) {
                    throw new ArgumentException(string.Format("Index must be between 0 and {0}", Count - 1));
                }
                return null;
            }
            T t = new T();
            t._index = index;
            return t;
        }

        public static T ByName(string name) {
            if (!IsDefinedName(name)) {
                if (_allowInstanceExceptions) {
                    throw new ArgumentException(string.Format("'{0}' is not a defined name of {1}", name, typeof(T).Name));
                }
                return null;
            }
            T t = new T();
            t._index = _names.IndexOf(name);
            return t;
        }

        public static T ByValue(U value) {
            if (!IsDefinedValue(value)) {
                if (_allowInstanceExceptions) {
                    throw new ArgumentException(string.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
                }
                return null;
            }
            T t = new T();
            t._index = _values.IndexOf(value);
            return t;
        }

        public static int FirstIndexWith(U value) {
            int index = _values.IndexOf(value);
            if (index >= 0) {
                return index;
            }
            throw new ArgumentException(string.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
        }

        public static string FirstNameWith(U value) {
            int index = _values.IndexOf(value);
            if (index >= 0) {
                return _names[index];
            }
            throw new ArgumentException(string.Format("'{0}' is not a defined value of {1}", value, typeof(T).Name));
        }

        public static int[] GetIndices(U value) {
            List<int> indexList = new List<int>();
            for (int i = 0; i < _values.Count; i++) {
                if (_values[i].Equals(value)) {
                    indexList.Add(i);
                }
            }
            return indexList.ToArray();
        }

        public static List<string> GetNames() {
            return _names;
        }

        public static string[] GetNames(U value) {
            List<string> nameList = new List<string>();
            for (int i = 0; i < _values.Count; i++) {
                if (_values[i].Equals(value)) {
                    nameList.Add(_names[i]);
                }
            }
            return nameList.ToArray();
        }

        public static U[] GetValues() {
            return _values.ToArray();
        }

        public static int IndexOf(string name) {
            return _names.IndexOf(name);
        }

        public static bool IsDefinedIndex(int index) {
            if (index >= 0 && index < Count) {
                return true;
            }
            return false;
        }

        public static bool IsDefinedName(string name) {
            if (_names.IndexOf(name) >= 0) {
                return true;
            }
            return false;
        }

        public static bool IsDefinedValue(U value) {
            if (_values.IndexOf(value) >= 0) {
                return true;
            }
            return false;
        }

        public static string GetNameAt(int index) {
            if (index >= 0 && index < Count) {
                return _names[index];
            }
            throw new IndexOutOfRangeException(string.Format("Index must be between 0 and {0}", Count - 1));
        }

        public static string GetIdAt(int index) {
            if (index >= 0 && index < Count) {
                return _typeName + _names[index];
            }
            throw new IndexOutOfRangeException(string.Format("Index must be between 0 and {0}", Count - 1));
        }

        public static int GetAssociatedValue(int index) {
            if (index >= 0 && index < Count) {
                return _associatedValues[index];
            }
            throw new IndexOutOfRangeException(string.Format("Index must be between 0 and {0}", Count - 1));
        }

        public static string GetDescriptionAt(int index) {
            if (index >= 0 && index < Count) {
                return _descriptions[index];
            }
            throw new IndexOutOfRangeException(string.Format("Index must be between 0 and {0}", Count - 1));
        }

        public override string ToString() {
            return _names[_index];
        }

        public static U ValueAt(int index) {
            if (index >= 0 && index < Count) {
                return _values[index];
            }
            throw new IndexOutOfRangeException(string.Format("Index must be between 0 and {0}", Count - 1));
        }

        public static U ValueOf(string name) {
            int index = _names.IndexOf(name);
            if (index >= 0) {
                return _values[index];
            }
            throw new ArgumentException(string.Format("'{0}' is not a defined name of {1}", name, typeof(T).Name));
        }

        public static bool TryValueOf(string name, out U val) {
            int index = _names.IndexOf(name);
            if (index >= 0) {
                val = _values[index];
                return true;
            }
            val = default(U);
            return false;
        }

        private static readonly ValueDropdownList<U> _easyList;

        public static ValueDropdownList<U> GetDropdownList() {
            return _easyList;
        }

        //public static Componenc<BaseStat> GetBaseStat(IEntity owner) {
        //    var stats = new BaseStat[Count];
        //    for (int i = 0; i < stats.Length; i++) {
        //        stats[i] = owner.Add(new BaseStat(GetDefaultValue(i), GetIdAt(i)));

        //    }
        //    return new EntityCollection<BaseStat>(_typeName.Substring(0, _typeName.Length - 1), stats);
        //}

        //public static EntityCollection<VitalStat> GetVitalStat(IEntity owner) {
        //    var stats = new VitalStat[Count];
        //    for (int i = 0; i < stats.Length; i++) {
        //        stats[i] = owner.Add(new VitalStat(GetDefaultValue(i), GetIdAt(i)));

        //    }
        //    return new EntityCollection<VitalStat>(_typeName.Substring(0, _typeName.Length - 1), stats);
        //}
    }

}