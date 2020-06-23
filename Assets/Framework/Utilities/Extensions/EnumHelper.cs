using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using PixelComrades;

public static class EnumHelper {

    private static Dictionary<Type, SortedList<int, string>> _enumStrings = new Dictionary<Type, SortedList<int, string>>();
    private static Dictionary<Type, SortedList<int, string>> _enumDescr = new Dictionary<Type, SortedList<int, string>>();

    public static string GetString<T>(int enumIndex) where T : struct, IConvertible {
        string value;
        return GetEnumList<T>().TryGetValue(enumIndex, out value) ? value : "";
    }

    public static int GetLength<T>() where T : struct, IConvertible {
        var list = GetEnumList<T>();
        return list.Count;
    }

    public static T GetRandomEnum<T>() where T : struct, IConvertible {
        //var list = GetEnumList<T>();
        //var number = list.Keys[UnityEngine.Random.Range(0, list.Count)];
        //if (!typeof(T).IsEnum) {
        //    return (typeof(T));
        //}
        System.Array a = System.Enum.GetValues(typeof(T));
        T v = (T)a.GetValue(UnityEngine.Random.Range(0, a.Length));
        return v;
    }

    public static SortedList<int, string> GetEnumList<T>() {
        if (!typeof(T).IsEnum) {
            return null;
        }
        SortedList<int, string> dict;
        if (_enumStrings.TryGetValue(typeof(T), out dict)) {
            return dict;
        }
        var strings = AddEnumDictionary<T>();
        _enumStrings.Add(typeof(T), strings);
        return strings;
    }

    private static SortedList<int, string> AddEnumDictionary<T>() {
        var values = Enum.GetValues(typeof(T));
        var dict = new SortedList<int, string>();
        foreach (var value in values) {
            dict.Add(Convert.ToInt32(value), Enum.GetName(typeof(T), value));
        }
        return dict;
    }

    public static string GetDescription<T>(T en) where T : struct, IConvertible {
        string value;
        return GetEnumDescrList<T>().TryGetValue(en.ToInt32(CultureInfo.InvariantCulture), out value) ? value : "";
    }

    public static string ToDescription<T>(this T en) where T : struct, IConvertible {
        string value;
        return GetEnumDescrList<T>().TryGetValue(en.ToInt32(CultureInfo.InvariantCulture), out value) ? value : "";
    }

    public static string GetDescription<T>(int enumIndex) where T : struct, IConvertible {
        string value;
        return GetEnumDescrList<T>().TryGetValue(enumIndex, out value) ? value : "";
    }

    public static SortedList<int, string> GetEnumDescrList<T>() {
        SortedList<int, string> dict;
        if (_enumDescr.TryGetValue(typeof(T), out dict)) {
            return dict;
        }
        var strings = AddEnumDescrDictionary<T>();
        _enumDescr.Add(typeof(T), strings);
        return strings;
    }

    private static SortedList<int, string> AddEnumDescrDictionary<T>() {
        var values = Enum.GetValues(typeof(T));
        var dict = new SortedList<int, string>();
        foreach (var value in values) {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            dict.Add((int)value, attribute == null ? value.ToString() : attribute.Description);
        }
        return dict;
    }

    public static bool TryParse<T>(this Enum theEnum, string valueToParse, out T returnValue) {
        returnValue = default(T);
        int intEnumValue;
        if (Int32.TryParse(valueToParse, out intEnumValue)) {
            if (Enum.IsDefined(typeof(T), intEnumValue)) {
                returnValue = (T) (object) intEnumValue;
                return true;
            }
        }
        var names = EnumHelper.GetEnumList<T>();
        var fullName = string.Format("{0}.{1}", (typeof(T).Name), valueToParse);
        foreach (var name in names) {
            if (valueToParse.CompareCaseInsensitive(name.Value)) {
                returnValue = (T) (object) name.Key;
                return true;
            }
            if (fullName.CompareCaseInsensitive(name.Value)) {
                returnValue = (T) (object) name.Key;
                return true;
            }
        }
        return false;
    }

    public static bool TryParse<T>(string valueToParse, out T returnValue) {
        returnValue = default(T);
        var type = typeof(T);
        if (type == typeof(string)) {
            return false;
        }
        
        if (Int32.TryParse(valueToParse, out var intEnumValue)) {
            if (Enum.IsDefined(type, intEnumValue)) {
                returnValue = (T) (object) intEnumValue;
                return true;
            }
        }
        var names = EnumHelper.GetEnumList<T>();
        if (names == null) {
            return false;
        }
        var fullName = type.Name + "." + valueToParse;
        foreach (var name in names) {
            if (valueToParse.CompareCaseInsensitive(name.Value)) {
                returnValue = (T) Enum.ToObject(typeof(T), name.Key);
                //returnValue = (T) (object) name.Key;
                return true;
            }
            if (fullName.CompareCaseInsensitive(name.Value)) {
                returnValue = (T) Enum.ToObject(typeof(T), name.Key);
                //returnValue = (T) (object) name.Key;
                return true;
            }
        }
        return false;
    }

    public static T ForceParse<T>(string valueToParse) {
        var returnValue = default(T);
        int intEnumValue;
        if (Int32.TryParse(valueToParse, out intEnumValue)) {
            if (Enum.IsDefined(typeof(T), intEnumValue)) {
                returnValue = (T) (object) intEnumValue;
                return returnValue;
            }
        }
        var names = EnumHelper.GetEnumList<T>();
        var fullName = string.Format("{0}.{1}", (typeof(T).Name), valueToParse);
        foreach (var name in names) {
            if (valueToParse.CompareCaseInsensitive(name.Value)) {
                returnValue = (T) (object) name.Key;
                return returnValue;
            }
            if (fullName.CompareCaseInsensitive(name.Value)) {
                returnValue = (T) (object) name.Key;
                return returnValue;
            }
        }
        return returnValue;
    }
}