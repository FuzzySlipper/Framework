using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelComrades {
    public static partial class GameData {

        private static string[] _fileExtensions = new []{".cdb", ".json"};
        private static string _mainJsonPath = Application.streamingAssetsPath + "/GameData";
        private static Enums _enums = new Enums();
        private static Dictionary<string, Dictionary<string, DataEntry>> _sheets = new Dictionary<string, Dictionary<string, DataEntry>>();
        private static Dictionary<string, DataEntry> _entriesByFullID = new Dictionary<string, DataEntry>();

        public static Enums Enums { get { return _enums; } }
        public static FakeEnum Vitals { get { return _enums[StatTypes.Vitals]; } }
        public static FakeEnum Attributes { get { return _enums[StatTypes.Attributes]; } }
        public static FakeEnum DamageTypes { get { return _enums[StatTypes.DamageTypes]; } }

        private static System.Action _onInit;

        public static void AddInit(System.Action action) {
            _onInit -= action;
            _onInit += action;
        }

        public static void Init() {
            _sheets.Clear();
            _entriesByFullID.Clear();
            _enums = new Enums();
            var allowedExtensions = new HashSet<string>(_fileExtensions, StringComparer.OrdinalIgnoreCase);
            var files = new DirectoryInfo(_mainJsonPath).EnumerateFiles().Where(f => allowedExtensions.Contains(f.Extension));
            foreach (var file in files) {
                var db = new JsonDB(File.ReadAllText(file.FullName));
                foreach (var dbSheet in db.Sheets) {
                    Dictionary<string, DataEntry> sheet;
                    if (!_sheets.TryGetValue(dbSheet.Key, out sheet)) {
                        sheet = new Dictionary<string, DataEntry>();
                        _sheets.Add(dbSheet.Key, sheet);
                    }
                    for (int i = 0; i < dbSheet.Value.Count; i++) {
                        var entry = dbSheet.Value[i];
                        sheet.SafeAdd(entry.ID, entry);
                        _entriesByFullID.SafeAdd(entry.FullID, entry);
                    }
                }
            }
            _onInit.SafeInvoke();
        }

        public static DataEntry GetData(string sheetName, string entryID) {
            if (!_sheets.TryGetValue(sheetName, out var sheet)) {
                return null;
            }
            return sheet.TryGetValue(entryID, out var entry) ? entry : null;
        }

        public static DataEntry GetData(string fullEntryID) {
            return _entriesByFullID.TryGetValue(fullEntryID, out var entry) ? entry : null;
        }

        public static Dictionary<string, DataEntry> GetSheet(string sheetName) {
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
