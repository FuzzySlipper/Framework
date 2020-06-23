using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelComrades {
    public static partial class GameData {

        private static HashSet<string> _fileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cdb", ".json" };
        private static string _mainJsonPath = Application.streamingAssetsPath + "/GameData";
        private static Enums _enums = new Enums();
        private static Dictionary<string, Dictionary<string, DataEntry>> _sheets = new Dictionary<string, Dictionary<string, DataEntry>>();
        private static Dictionary<string, DataEntry> _entriesByFullID = new Dictionary<string, DataEntry>();

        public static Enums Enums { get { return _enums; } }
<<<<<<< HEAD
        public static FakeEnum Vitals { get { return _enums[StatTypes.Vitals]; } }
        public static FakeEnum Attributes { get { return _enums[StatTypes.Attributes]; } }
        public static FakeEnum DamageTypes { get { return _enums[StatTypes.DamageTypes]; } }
        public static FakeEnum EquipmentSlotTypes { get { return _enums[EnumTypes.EquipmentSlotType]; } }
        public static FakeEnum Skills { get { return _enums[StatTypes.Skills]; } }
        public static FakeEnum Classes { get { return _enums[StatTypes.Classes]; } }

=======
>>>>>>> FirstPersonAction
        
        private static System.Action _onInit;

        public static void AddInit(System.Action action) {
            _onInit -= action;
            _onInit += action;
        }

        public static void Init() {
            _sheets.Clear();
            _entriesByFullID.Clear();
            _enums = new Enums();
            var files = new DirectoryInfo(_mainJsonPath).EnumerateFiles().Where(f => _fileExtensions.Contains(f.Extension));
            foreach (var file in files) {
                var db = new JsonDB(File.ReadAllText(file.FullName));
                foreach (var dbSheet in db.Sheets) {
                    if (!_sheets.TryGetValue(dbSheet.Key, out var sheet)) {
                        sheet = new Dictionary<string, DataEntry>();
                        _sheets.Add(dbSheet.Key, sheet);
                    }
                    for (int i = 0; i < dbSheet.Value.Count; i++) {
                        var entry = dbSheet.Value[i];
                        sheet.AddOrUpdate(entry.ID, entry);
                        _entriesByFullID.AddOrUpdate(entry.FullID, entry);
                    }
                }
            }
            _onInit.SafeInvoke();
        }

        public static DataEntry GetData(string sheetName, string entryID) {
            if (_sheets.Count == 0) {
                Init();
            }
            if (!_sheets.TryGetValue(sheetName, out var sheet)) {
                return null;
            }
            return sheet.TryGetValue(entryID, out var entry) ? entry : null;
        }

        public static DataEntry GetData(string fullEntryID) {
            if (_sheets.Count == 0) {
                Init();
            }
            return _entriesByFullID.TryGetValue(fullEntryID, out var entry) ? entry : null;
        }

        public static Dictionary<string, DataEntry> GetSheet(string sheetName) {
            if (_sheets.Count == 0) {
                Init();
            }
            return _sheets.TryGetValue(sheetName, out var list) ? list : null;
        }

        public static T Get<T>(string sheetName, string entryID, string field) {
            var entry = GetData(sheetName, entryID);
            if (entry == null) {
                return default(T);
            }
            var cell = entry.Get(field);
            if (cell == null) {
                return default(T);
            }
            if (cell is DataCell<T> typeCell) {
                return typeCell.Value;
            }
            return (T) cell.Get;
        }
    }
}
