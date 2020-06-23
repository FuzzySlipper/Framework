using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades {
    public class GameOptions : SimpleScriptableDatabase<GameOptions> {
        
        private const string SheetName = "GameOptions";
        private const string Row = "Default";

        [SerializeField] private KeyedFloatValue[] _floatValues = new KeyedFloatValue[0];
        [SerializeField] private KeyedIntValue[] _intValues = new KeyedIntValue[0];
        [SerializeField] private KeyedBoolValue[] _boolValues = new KeyedBoolValue[0];
        [SerializeField] private KeyedStringValue[] _stringValues = new KeyedStringValue[0];
        [SerializeField] private KeyedColorValue[] _colorValues = new KeyedColorValue[0];

        private static Dictionary<string, KeyedIntValue> _dictInt = new Dictionary<string, KeyedIntValue>();
        private static Dictionary<string, KeyedFloatValue> _dictFloat = new Dictionary<string, KeyedFloatValue>();
        private static Dictionary<string, KeyedBoolValue> _dictBool = new Dictionary<string, KeyedBoolValue>();
        private static Dictionary<string, KeyedStringValue> _dictString = new Dictionary<string, KeyedStringValue>();
        private static Dictionary<string, KeyedColorValue> _dictColor = new Dictionary<string, KeyedColorValue>();

        [Button, Command("initGameOptions")]
        public static void Init() {
            GameData.AddInit(Init);
            Main.RebuildDictionaries();
            Cached.Reset();
        }

        [Button]
        public void LoadFromJson() {
            Init();
            var row = GameData.GetSheet(SheetName)[Row];
            LoadDictionary<string, KeyedStringValue>(row.Get<DataList>("Text"), _dictString, ref Main._stringValues);
            LoadDictionary<bool, KeyedBoolValue>(row.Get<DataList>("Bool"), _dictBool, ref Main._boolValues);
            LoadDictionary<int, KeyedIntValue>(row.Get<DataList>("Int"), _dictInt, ref Main._intValues);
            LoadDictionary<float, KeyedFloatValue>(row.Get<DataList>("Float"), _dictFloat, ref Main._floatValues);
            LoadDictionary<Color, KeyedColorValue>(row.Get<DataList>("Color"), _dictColor, ref Main._colorValues);
        }

        public static Dictionary<string, KeyedIntValue> DictInt { get => _dictInt; set => _dictInt = value; }
        public static Dictionary<string, KeyedFloatValue> DictFloat { get => _dictFloat; set => _dictFloat = value; }
        public static Dictionary<string, KeyedBoolValue> DictBool { get => _dictBool; set => _dictBool = value; }
        public static Dictionary<string, KeyedStringValue> DictString { get => _dictString; set => _dictString = value; }
        public static Dictionary<string, KeyedColorValue> DictColor { get => _dictColor; set => _dictColor = value; }

        public void OnValidate() {
            RebuildDictionaries();
        }

        private void RebuildDictionaries() {
            FillDictionary(_intValues, _dictInt);
            FillDictionary(_floatValues, _dictFloat);
            FillDictionary(_boolValues, _dictBool);
            FillDictionary(_stringValues, _dictString);
            FillDictionary(_colorValues, _dictColor);
        }

        private void FillDictionary<T>(T[] array, Dictionary<string, T> dict) where T : GenericKeyedValue {
            dict.Clear();
            for (int i = 0; i < array.Length; i++) {
                if (array[i] == null || string.IsNullOrEmpty(array[i].Key)) {
                    continue;
                }
                dict.AddOrUpdate(array[i].Key, array[i]);
            }
        }

        [Command("SetIntOption")]
        public static void SetIntOption(string key, int value) {
            if (_dictInt.ContainsKey(key)) {
                _dictInt[key].Value = value;
            }
            else {
                _dictInt.Add(key, new KeyedIntValue());
            }
            Cached.Reset();
        }

        [Command("SetFloatOption")]
        public static void SetFloatOption(string key, float value) {
            if (_dictFloat.ContainsKey(key)) {
                _dictFloat[key].Value = value;
            }
            else {
                _dictFloat.Add(key, new KeyedFloatValue());
            }
            Cached.Reset();
        }

        [Command("SetBoolOption")]
        public static void SetBoolOption(string key, bool value) {
            if (_dictBool.ContainsKey(key)) {
                _dictBool[key].Value = value;
            }
            else {
                _dictBool.Add(key, new KeyedBoolValue());
            }
            Cached.Reset();
        }

        [Command("SetStringOption")]
        public static void SetStringOption(string key, string value) {
            if (_dictString.ContainsKey(key)) {
                _dictString[key].Value = value;
            }
            else {
                _dictString.Add(key, new KeyedStringValue());
            }
            Cached.Reset();
        }

        [Command("SetColorOption")]
        public static void SetColorOption(string key, Color value) {
            if (_dictColor.ContainsKey(key)) {
                _dictColor[key].Value = value;
            }
            else {
                _dictColor.Add(key, new KeyedColorValue());
            }
            Cached.Reset();
        }

        private static void LoadDictionary<T,TV>(DataList data, Dictionary<string, TV> dict, ref TV[] array) where TV : GenericKeyedValue {
            for (int i = 0; i < data.Count; i++) {
                if (!data[i].TryGetValue(DatabaseFields.ID, out string id)) {
                    continue;
                }
                if (!data[i].TryGetValue(DatabaseFields.Value, out T value)) {
                    continue;
                }
                if (dict.TryGetValue(id, out var valueHolder)) {
                    valueHolder.ValueAccess = value;
                }
                else {
                    System.Array.Resize(ref array, array.Length + 1);
                    valueHolder = (TV) GenericKeyedValue.New(id, value);
                    array[array.LastIndex()] = valueHolder;
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(Main);
                    #endif
                    dict.Add(id, valueHolder);
                }
            }
        }

        public static string Get(string id, string defaultValue) {
            if (_dictString.Count == 0) {
                Init();
            }
            return _dictString.TryGetValue(id, out var value) ? value.Value : defaultValue;
        }

        public static float Get(string id, float defaultValue) {
            if (_dictFloat.Count == 0) {
                Init();
            }
            if (_dictFloat.TryGetValue(id, out var value)) {
                return value.Value;
            }
            if (_dictInt.TryGetValue(id, out var intValue)) {
                return (float) intValue.Value;
            }
            Debug.Log(id);
            return defaultValue;
        }

        public static int Get(string id, int defaultValue) {
            if (_dictInt.Count == 0) {
                Init();
            }
            if (_dictInt.TryGetValue(id, out var intValue)) {
                return intValue.Value;
            }
            if (_dictFloat.TryGetValue(id, out var fValue)) {
                return (int) fValue.Value;
            }
            Debug.Log(id);
            return defaultValue;
        }

        public static bool Get(string id, bool defaultValue) {
            if (_dictBool.Count == 0) {
                Init();
            }
            return _dictBool.TryGetValue(id, out var value) ? value.Value : defaultValue;
        }

        public static Color Get(string id, Color defaultValue) {
            if (_dictColor.Count == 0) {
                Init();
            }
            return _dictColor.TryGetValue(id, out var value) ? value.Value : defaultValue;
        }

        public static void Set(string id, float value) {
            SetFloatOption(id, value);
        }

        public static void Set(string id, bool value) {
            SetBoolOption(id, value);
        }

        public static void Set(string id, Color value) {
            SetColorOption(id, value);
        }


        public static int PriceEstimateSell(Entity item) {
            var inven = item.Get<InventoryItem>();
            if (inven == null) {
                return 100;
            }
            return (int) (inven.Price * (inven.Identified ? 1 : Get(RpgSettings.UnidentifiedSaleModifier, 1f)));
        }

        public static int RepairEstimate(Entity item) {
            return PriceEstimateSell(item);
        }

        public static int IdentifyEstimate(Entity item) {
            return 100 * (int) item.Get<StatsContainer>().Get(Stat.Level).Value;
        }

        public static CachedBool UseShaking = new CachedBool("UseShaking");
        public static CachedBool UsePainFlash = new CachedBool("UsePainFlash");
        public static CachedBool VerboseInventory = new CachedBool("VerboseInventory");
        public static CachedBool LogAllDamage = new CachedBool("LogAllDamage");
        public static CachedBool ShowMiss = new CachedBool("ShowMiss");
        public static CachedBool PauseForInput = new CachedBool("PauseForInput");
        public static CachedBool ReadyNotice = new CachedBool("ReadyNotice");
        public static CachedBool DebugMode = new CachedBool("DebugMode");
        public static CachedBool LoadSceneDestructive = new CachedBool("LoadSceneDestructive");

        private const string MouseLookLabel = "MouseLook";
        public static bool MouseLook {
            get { return Get(MouseLookLabel, false); }
            set {
                if (Get(MouseLookLabel, false) == value) {
                    return;
                }
                Set(MouseLookLabel, value);
                if (value && !Game.CursorUnlocked) {
                    Cursor.lockState = CursorLockMode.Locked;
                    UICursor.main.SetCursor(UICursor.CrossHair);
                }
                else if (!value) {
                    Cursor.lockState = CursorLockMode.None;
                    CameraSystem.CamTr.localRotation = Quaternion.identity;
                }
            }
        }

        private const string TurnBasedLabel = "TurnBased";
        public static bool TurnBased {
            get { return Get(TurnBasedLabel, false); }
            set {
                if (Get(TurnBasedLabel, false) == value) {
                    return;
                }
                Set(TurnBasedLabel, value);
                MessageKit.post(Messages.TurnBasedChanged);
            }
        }

        public class Cached {

            private static List<Cached> _cached = new List<Cached>();

            public static void Reset() {
                for (int i = 0; i < _cached.Count; i++) {
                    if (_cached[i] == null) {
                        continue;
                    }
                    _cached[i].ValueSet = false;
                }
            }

            protected readonly string Key;
            protected bool ValueSet = false;

            protected Cached(string key) {
                Key = key;
                _cached.Add(this);
            }
        }

        public class CachedInt : Cached {
            private KeyedIntValue _value;

            public CachedInt(string key) : base(key) {}

            public int Value {
                get {
                    if (!ValueSet) {
                        if (_dictInt.TryGetValue(Key, out _value)) {
                            ValueSet = true;
                        }
                        else {
                            return -1;
                        }
                    }
                    return _value.Value;
                }
            }

            public static implicit operator int(CachedInt cached) {
                return cached.Value;
            }

            public static bool operator ==(CachedInt cached, int value) {
                return cached != null && cached.Value == value;
            }

            public static bool operator !=(CachedInt cached, int value) {
                return cached != null && cached.Value != value;
            }

            protected bool Equals(CachedInt other) {
                return other != null && Value == other.Value;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                return Equals((CachedInt) obj);
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        public class CachedFloat : Cached {
            private KeyedFloatValue _value;

            public CachedFloat(string key) : base(key) {}

            public float Value {
                get {
                    if (!ValueSet) {
                        if (_dictFloat.TryGetValue(Key, out _value)) {
                            ValueSet = true;
                        }
                        else {
                            return -1;
                        }
                    }
                    return _value.Value;
                }
            }

            private const float Tolerance = 0.0001f;

            public static implicit operator float(CachedFloat cached) {
                return cached.Value;
            }

            public static bool operator ==(CachedFloat cached, float value) {
                return cached != null && Math.Abs(cached.Value - value) < Tolerance;
            }

            public static bool operator !=(CachedFloat cached, float value) {
                return cached != null && Math.Abs(cached.Value - value) > Tolerance;
            }

            protected bool Equals(CachedFloat other) {
                return other != null && Math.Abs(Value - other.Value) < Tolerance;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                return Equals((CachedFloat) obj);
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        public class CachedBool : Cached {
            private KeyedBoolValue _value;

            public CachedBool(string key) : base(key) {}

            public bool Value {
                get {
                    if (!ValueSet) {
                        if (_dictBool.TryGetValue(Key, out _value)) {
                            ValueSet = true;
                        }
                        else {
                            return false;
                        }
                    }
                    return _value.Value;
                }
                set {
                    if (_value.Value != value) {
                        Set(Key, value);
                    }
                }
            }

            public static implicit operator bool(CachedBool cached) {
                return cached.Value;
            }

            public static bool operator ==(CachedBool cached, bool value) {
                return cached != null && cached.Value == value;
            }

            public static bool operator !=(CachedBool cached, bool value) {
                return cached != null && cached.Value != value;
            }

            protected bool Equals(CachedBool other) {
                return other != null && Value == other.Value;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                return Equals((CachedBool) obj);
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }

        public class CachedString : Cached {
            private KeyedStringValue _value;

            public CachedString(string key) : base(key) {}

            public string Value {
                get {
                    if (!ValueSet) {
                        if (_dictString.TryGetValue(Key, out _value)) {
                            ValueSet = true;
                        }
                        else {
                            return "";
                        }
                    }
                    return _value.Value;
                }
            }

            public static implicit operator string(CachedString cached) {
                return cached.Value;
            }

            public static bool operator ==(CachedString cached, string value) {
                return cached != null && cached.Value == value;
            }

            public static bool operator !=(CachedString cached, string value) {
                return cached != null && cached.Value != value;
            }

            protected bool Equals(CachedString other) {
                return other != null && string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                return Equals((CachedString) obj);
            }

            public override int GetHashCode() {
                return (!string.IsNullOrEmpty(Value) ? Value.GetHashCode() : Key.GetHashCode());
            }
        }

        public class Cached<T> : CachedString {

            private Func<string, T> _parseDel;
            private T _value;
            private bool _parsed;

            public Cached(string key, Func<string, T> parseDel) : base(key) {
                _parseDel = parseDel;
            }

            public T ParsedValue {
                get {
                    if (_parsed) {
                        return _value;
                    }
                    if (string.IsNullOrEmpty(Value)) {
                        return default(T);
                    }
                    _value = _parseDel(Value);
                    _parsed = true;
                    return _value;
                }
            }
        }

        public class CachedColor : Cached {
            private KeyedColorValue _value;

            public CachedColor(string key) : base(key) {
            }

            public Color Value {
                get {
                    if (!ValueSet) {
                        if (_dictColor.TryGetValue(Key, out _value)) {
                            ValueSet = true;
                        }
                        else {
                            return Color.clear;
                        }
                    }
                    return _value.Value;
                }
                set {
                    Set(Key, value);
                }
            }

            public static implicit operator Color(CachedColor cached) {
                return cached.Value;
            }

            public static bool operator ==(CachedColor cached, Color value) {
                return cached != null && cached.Value == value;
            }

            public static bool operator !=(CachedColor cached, Color value) {
                return cached != null && cached.Value != value;
            }

            protected bool Equals(CachedColor other) {
                return other != null && Value == other.Value;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }
                if (ReferenceEquals(this, obj)) {
                    return true;
                }
                return Equals((CachedColor) obj);
            }

            public override int GetHashCode() {
                return Value.GetHashCode();
            }
        }
    }

    [System.Serializable]
    public abstract class GenericKeyedValue {
        public string Key;

        public static GenericKeyedValue New(string key, System.Object value) {
            if (value is float floatValue) {
                return new KeyedFloatValue(key, floatValue);
            }
            if (value is int intValue) {
                return new KeyedIntValue(key, intValue);
            }
            if (value is string stringValue) {
                return new KeyedStringValue(key, stringValue);
            }
            if (value is bool boolValue) {
                return new KeyedBoolValue(key, boolValue);
            }
            if (value is Color colorValue) {
                return new KeyedColorValue(key, colorValue);
            }
            return null;
        }
        public abstract System.Object ValueAccess { get; set; }
    }

    [System.Serializable]
    public class KeyedFloatValue : GenericKeyedValue {
        public float Value;
        public KeyedFloatValue() { }

        public KeyedFloatValue(string key, float value) {
            Value = value;
            Key = key;
        }

        public override object ValueAccess {
            get {
                return Value;
            }
            set {
                if (value is float newValue) {
                    Value = newValue;
                }
            }
        }
    }

    [System.Serializable]
    public class KeyedIntValue : GenericKeyedValue {
        public int Value;
        public KeyedIntValue() { }

        public KeyedIntValue(string key, int value) {
            Value = value;
            Key = key;
        }

        public override object ValueAccess {
            get {
                return Value;
            }
            set {
                if (value is int newValue) {
                    Value = newValue;
                }
            }
        }
    }

    [System.Serializable]
    public class KeyedBoolValue : GenericKeyedValue {
        public bool Value;
        public KeyedBoolValue() { }

        public KeyedBoolValue(string key, bool value) {
            Value = value;
            Key = key;
        }

        public override object ValueAccess {
            get {
                return Value;
            }
            set {
                if (value is bool newValue) {
                    Value = newValue;
                }
            }
        }
    }

    [System.Serializable]
    public class KeyedStringValue : GenericKeyedValue {
        public string Value;
        public KeyedStringValue() { }

        public KeyedStringValue(string key, string value) {
            Value = value;
            Key = key;
        }

        public override object ValueAccess {
            get {
                return Value;
            }
            set {
                if (value is string newValue) {
                    Value = newValue;
                }
            }
        }
    }

    [System.Serializable]
    public class KeyedColorValue : GenericKeyedValue {
        public Color Value;
        public KeyedColorValue() { }

        public KeyedColorValue(string key, Color value) {
            Value = value;
            Key = key;
        }

        public override object ValueAccess {
            get {
                return Value;
            }
            set {
                if (value is Color newValue) {
                    Value = newValue;
                }
            }
        }
    }
}
