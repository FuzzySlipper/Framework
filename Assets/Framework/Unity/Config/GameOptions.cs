using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class GameOptions {
        
        private const string SheetName = "GameOptions";
        private const string Row = "Default";

        private static Dictionary<string, int> _dictInt = new Dictionary<string, int>();
        private static Dictionary<string, float> _dictFloat = new Dictionary<string, float>();
        private static Dictionary<string, bool> _dictBool = new Dictionary<string, bool>();
        private static Dictionary<string, string> _dictString = new Dictionary<string, string>();
        private static Dictionary<string, Color> _dictColor = new Dictionary<string, Color>();

        [Command("initGameOptions")]
        public static void Init() {
            GameData.AddInit(Init);
            var row = GameData.GetSheet(SheetName)[Row];
            LoadDictionary(row.Get<DataList>("Text"), _dictString);
            LoadDictionary(row.Get<DataList>("Bool"), _dictBool);
            LoadDictionary(row.Get<DataList>("Int"), _dictInt);
            LoadDictionary(row.Get<DataList>("Float"), _dictFloat);
            LoadDictionary(row.Get<DataList>("Color"), _dictColor);
            Cached.Reset();
        }

        [Command("SetIntOption")]
        public static void SetIntOption(string key, int value) {
            if (_dictInt.ContainsKey(key)) {
                _dictInt[key] = value;
            }
            else {
                _dictInt.Add(key, value);
            }
            Cached.Reset();
        }

        [Command("SetFloatOption")]
        public static void SetFloatOption(string key, float value) {
            if (_dictFloat.ContainsKey(key)) {
                _dictFloat[key] = value;
            }
            else {
                _dictFloat.Add(key, value);
            }
            Cached.Reset();
        }

        [Command("SetBoolOption")]
        public static void SetBoolOption(string key, bool value) {
            if (_dictBool.ContainsKey(key)) {
                _dictBool[key] = value;
            }
            else {
                _dictBool.Add(key, value);
            }
            Cached.Reset();
        }

        [Command("SetStringOption")]
        public static void SetStringOption(string key, string value) {
            if (_dictString.ContainsKey(key)) {
                _dictString[key] = value;
            }
            else {
                _dictString.Add(key, value);
            }
            Cached.Reset();
        }

        [Command("SetColorOption")]
        public static void SetColorOption(string key, Color value) {
            if (_dictColor.ContainsKey(key)) {
                _dictColor[key] = value;
            }
            else {
                _dictColor.Add(key, value);
            }
            Cached.Reset();
        }

        private static void LoadDictionary<T>(DataList data, Dictionary<string, T> dict) {
            dict.Clear();
            for (int i = 0; i < data.Count; i++) {
                if (!data[i].TryGetValue(DatabaseFields.ID, out string id)) {
                    continue;
                }
                if (!data[i].TryGetValue(DatabaseFields.Value, out T value)) {
                    continue;
                }
                dict.Add(id, value);
            }
        }

        public static string Get(string id, string defaultValue) {
            if (_dictString.Count == 0) {
                Init();
            }
            return _dictString.TryGetValue(id, out var value) ? value : defaultValue;
        }

        public static float Get(string id, float defaultValue) {
            if (_dictFloat.Count == 0) {
                Init();
            }
            if (_dictFloat.TryGetValue(id, out var value)) {
                return value;
            }
            if (_dictInt.TryGetValue(id, out var intValue)) {
                return (float) intValue;
            }
            Debug.Log(id);
            return defaultValue;
        }

        public static int Get(string id, int defaultValue) {
            if (_dictInt.Count == 0) {
                Init();
            }
            if (_dictInt.TryGetValue(id, out var intValue)) {
                return intValue;
            }
            if (_dictFloat.TryGetValue(id, out var fValue)) {
                return (int) fValue;
            }
            Debug.Log(id);
            return defaultValue;
        }

        public static bool Get(string id, bool defaultValue) {
            if (_dictBool.Count == 0) {
                Init();
            }
            return _dictBool.TryGetValue(id, out var value) ? value : defaultValue;
        }

        public static Color Get(string id, Color defaultValue) {
            if (_dictColor.Count == 0) {
                Init();
            }
            return _dictColor.TryGetValue(id, out var value) ? value : defaultValue;
        }

        public static void Set(string id, string value) {
            _dictString.AddOrUpdate(id, value);
        }

        public static void Set(string id, int value) {
            _dictInt.AddOrUpdate(id, value);
        }

        public static void Set(string id, float value) {
            _dictFloat.AddOrUpdate(id, value);
        }

        public static void Set(string id, bool value) {
            _dictBool.AddOrUpdate(id, value);
        }

        public static void Set(string id, Color value) {
            _dictColor.AddOrUpdate(id, value);
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
            return 100 * item.Get<EntityLevelComponent>().Level;
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
                    Player.Cam.transform.localRotation = Quaternion.identity;
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
            private int _value;

            public CachedInt(string key) : base(key) {}

            public int Value {
                get {
                    if (!ValueSet) {
                        _value = Get(Key, 0);
                        ValueSet = true;
                    }
                    return _value;
                }
                set {
                    _value = value;
                    Set(Key, value);
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
            private float _value;

            public CachedFloat(string key) : base(key) {}

            public float Value {
                get {
                    if (!ValueSet) {
                        _value = Get(Key, 0f);
                        ValueSet = true;
                    }
                    return _value;
                }
                set {
                    _value = value;
                    Set(Key, value);
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
            private bool _value;

            public CachedBool(string key) : base(key) {}

            public bool Value {
                get {
                    if (!ValueSet) {
                        _value = Get(Key, false);
                        ValueSet = true;
                    }
                    return _value;
                }
                set {
                    _value = value;
                    Set(Key, value);
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
            private string _value;

            public CachedString(string key) : base(key) {}

            public string Value {
                get {
                    if (!ValueSet) {
                        _value = Get(Key, "");
                        ValueSet = true;
                    }
                    return _value;
                }
                set {
                    _value = value;
                    Set(Key, value);
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
            private Color _value;

            public CachedColor(string key) : base(key) {
            }

            public Color Value {
                get {
                    if (!ValueSet) {
                        _value = Get(Key, Color.white);
                        ValueSet = true;
                    }
                    return _value;
                }
                set {
                    _value = value;
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
}
