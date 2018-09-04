using System;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace PixelComrades.DungeonCrawler {

    public class ItemDatabase : GenericDatabase<ItemTemplate> {

        private static ItemDatabase _staticDatabase;
        public static ItemDatabase Instance {
            get {
                if (_staticDatabase == null) {
                    _staticDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
                    _staticDatabase.CreateShuffleBags();
                }
                return _staticDatabase;
            }
        }

        [SerializeField] private ItemTemplate _spellScroll = null;
        [SerializeField] private PrefabEntity _defaultGoldPickup = null;
        [SerializeField] private float _explosiveForce = 10;
        [SerializeField] private float _explosiveRadius = 5;
        [SerializeField] private float _percentChanceRarity = 10;
        [SerializeField] private List<ItemModifier> _itemModifiers = new List<ItemModifier>();
        [SerializeField] private ActionFx _defaultMeleeFx = null;
        
        private ShuffleBag<ItemTemplate>[] _maxRarityBags;
        private ShuffleBag<ItemTemplate>[] _specificRarityBags;
        private Dictionary<string, ItemModifier> _modifierDictionary = new Dictionary<string, ItemModifier>();
        private Dictionary<ModifierGroups, ShuffleBag<ItemModifier>> _prefixBag = new Dictionary<ModifierGroups, ShuffleBag<ItemModifier>>();
        private Dictionary<ModifierGroups, ShuffleBag<ItemModifier>> _suffixBag = new Dictionary<ModifierGroups, ShuffleBag<ItemModifier>>();


        protected override void ProcessData() {
            base.ProcessData();
            _prefixBag.Clear();
            _suffixBag.Clear();
            _modifierDictionary.Clear();
            for (int i = 0; i < _itemModifiers.Count; i++) {
                var mod = _itemModifiers[i];
                _modifierDictionary.Add(mod.Id, mod);
                for (int j = 0; j < mod.ValidTypes.Count; j++) {
                    var list = GetModList(mod.ValidTypes[j], mod.IsPrefix);
                    for (int c = 0; c < mod.Chance; c++) {
                        list.Add(mod);
                    }
                }
            }
        }

        protected ShuffleBag<ItemModifier> GetModList(ModifierGroups modGroup, bool isPrefix) {
            ShuffleBag<ItemModifier> list;
            var dict = isPrefix ? _prefixBag : _suffixBag;
            if (dict.TryGetValue(modGroup, out list)) {
                return list;
            }
            list = new ShuffleBag<ItemModifier>();
            dict.Add(modGroup, list);
            return list;
        }

        private void CreateShuffleBags() {
            var enumCount = 5; //item rarity enum count - Special
            _maxRarityBags = new ShuffleBag<ItemTemplate>[enumCount];
            _specificRarityBags = new ShuffleBag<ItemTemplate>[enumCount];
            List<ItemTemplate>[] rarity = new List<ItemTemplate>[enumCount];
            for (int i = 0; i < enumCount; i++) {
                rarity[i] = new List<ItemTemplate>();
            }
            RunActionOnData(template => {
                if (template.Rarity != ItemRarity.Special) {
                    var index = (int) template.Rarity;
                    if (index >= 0 && index <= rarity.Length - 1) {
                        rarity[index].Add(template);
                    }
                }
            });
            for (int i = 0; i < enumCount; i++) {
                _specificRarityBags[i] = new ShuffleBag<ItemTemplate>(rarity[i].ToArray());
                var megaList = new List<ItemTemplate>();
                megaList.AddRange(rarity[i]);
                if (i > 0) {
                    for (int j = i - 1; j >= 0; j--) {
                        megaList.AddRange(rarity[j]);
                    }
                }
                _maxRarityBags[i] = new ShuffleBag<ItemTemplate>(megaList.ToArray());
            }
        }

        private static void CheckBags() {
            if (Instance._maxRarityBags == null || Instance._maxRarityBags.Length == 0) {
                Instance.CreateShuffleBags();
            }
        }

        //public static CurrencyItem GetCurrencyItem(int amount) {
        //    var money = new CurrencyItem(amount);
        //    return money;
        //}

        public static void SpawnWorldGold(int amt, Vector3 position) {
            for (int i = 0; i < amt; i++) {
                var gold = ItemPool.Spawn(Instance._defaultGoldPickup, position + UnityEngine.Random.insideUnitSphere * 1.5f, Quaternion.identity,  true, true);
                gold.GetComponent<Rigidbody>().AddExplosionForce(Instance._explosiveForce, position,  Instance._explosiveRadius);
            }
        }

        public static Entity GetItem(string id) {
            CheckBags();
            var template = Instance.GetData(id);
            return template != null ? CreateItem(template) : null;
        }

        //public static bool TryEquip(PlayerCharacterNode actor, string id) {
        //    var item = GetItem(id);
        //    if (item != null) {
        //        return actor.EquipSlots.c.TryEquip(item);
        //    }
        //    return false;
        //}

        //public static void TryEquipOutfit(PlayerCharacterNode actor, string outfit) {
        //    for (int i = 1; i <= (int) EquipSlotType.Hands; i++) {
        //        var slot = (EquipSlotType) i;
        //        var id = string.Format("{0}_{1}", outfit, slot);
        //        var template = GetTemplate(id);
        //        if (template != null) {
        //            var item = template.New();
        //            if (item != null) {
        //                if (!actor.EquipSlots.c.TryEquip(item)) {
        //                    Player.MainInventory.Add(item);
        //                }
        //            }
        //        }
        //    }
        //}

        public static Entity CreateItem(ItemTemplate template) {
            return CreateItem(template, RpgSystem.ItemRandomCurrentLevel(), template.Rarity);
        }

        public static Entity CreateItem(ItemTemplate template, int level, int rarity) {
            ItemModifier prefix = null;
            ItemModifier suffix = null;
            if (Game.DiceRollSuccess(((int) rarity + 1) * Instance._percentChanceRarity)) {
                prefix = GetMod(level, template.ModifierGroup, true);
            }
            if (Game.DiceRollSuccess(((int) rarity + 1) * Instance._percentChanceRarity)) {
                suffix = GetMod(level, template.ModifierGroup, false);
            }
            return template.New(level, prefix, suffix);
        }

        public static ItemTemplate GetTemplate(string itemName) {
            return Instance.GetData(itemName);
        }

        public static ItemModifier GetMod(string id) {
            ItemModifier mod;
            return Instance._modifierDictionary.TryGetValue(id, out mod) ? mod : null;
        }

        public static ItemModifier GetMod(int level, ModifierGroups modGroup, bool isPrefix) {
            var list = Instance.GetModList(modGroup, isPrefix);
            if (list == null || list.Count == 0) {
                return null;
            }
            int looper = 0;
            while (looper < 1500) {
                var mod = list.Next();
                if (mod.MinLevel <= level) {
                    return mod;
                }
                looper++;
            }
            return null;
        }

        public static Entity RandomItem(int rarity, bool isMaxRare = true) {
            CheckBags();
            var shuffleBag = Instance.GetRarity(rarity, isMaxRare);
            if (shuffleBag != null) {
                var template = shuffleBag.Next();
                return CreateItem(template, RpgSystem.ItemRandomCurrentLevel(), rarity);
            }
            return null;
        }

        public static ItemTemplate RandomTemplate(int rarity, bool isMaxRare = true) {
            CheckBags();
            var shuffleBag = Instance.GetRarity(rarity, isMaxRare);
            if (shuffleBag != null) {
                return shuffleBag.Next();
            }
            return null;
        }

        public ShuffleBag<ItemTemplate> GetRarity(int rarity, bool isMaxRare) {
            int currentRarity = (int) rarity;
            var rarityCollection = isMaxRare ? Instance._maxRarityBags : Instance._specificRarityBags;
            while (true) {
                if (rarityCollection[currentRarity].Count > 0) {
                    return rarityCollection[currentRarity];
                }
                currentRarity -= 1;
                if (currentRarity < 0) {
                    return null;
                }
            }
        }

        public override void CheckForDuplicates() {
            base.CheckForDuplicates();
            _modifierDictionary.Clear();
            for (int i = 0; i < _itemModifiers.Count; i++) {
                var mod = _itemModifiers[i];
                if (_modifierDictionary.ContainsKey(mod.Id)) {
                    Debug.LogErrorFormat("Key {0} already in database", mod.Id);
                }
                else {
                    _modifierDictionary.Add(mod.Id, mod);
                }
            }
        }

        public IGenericImporter Importer;

        [Button("Test Create Item")]
        public void TestItemCreate() {
            ProcessData();
            CheckBags();
            var limiter = new WhileLoopLimiter(1500);
            ShuffleBag<ItemTemplate> templateList = null;
            while (limiter.Advance()) {
                templateList = GetRarity(UnityEngine.Random.Range(0, 5), false);
                if (templateList != null && templateList.Count > 0) {
                    break;
                }
            }
            if (templateList == null) {
                Debug.Log("couldn't find template");
                return;
            }
            var template = templateList.RandomElement();
            var level = UnityEngine.Random.Range(1, 10);
            var item = template.New(level, GetMod(level, template.ModifierGroup, true), GetMod(level, template.ModifierGroup, false));
            Debug.LogFormat("Item: {0} Level {1}", item.Get<LabelComponent>(), item.Get<InventoryItem>());
        }

        #if UNITY_EDITOR
        [SerializeField] private int _linesToSkip = 2;
        [SerializeField] private string _sheetUrl = "";
        [SerializeField] private string[] _sheetNames = new string[0];
        [SerializeField, Multiline] private string _pasteString = "";

        [Button("Import Paste String")]
        public void ProcessPasteList() {
            if (Importer == null) {
                return;
            }
            Importer.SetDatabase(this);
            RefreshAssets();
            ProcessListString(_pasteString, '\t');
            RefreshAssets();
            _pasteString = "";
        }

        [Button("Import Sheet")]
        public void ImportSheet() {
            if (Importer == null) {
                return;
            }
            Importer.SetDatabase(this);
            RefreshAssets();
            for (int i = 0; i < _sheetNames.Length; i++) {
                GoogleSheetsDownloadUtility.Download(ProcessListString, _sheetUrl, _sheetNames[i]);
            }
            UnityEditor.EditorUtility.SetDirty(this);
            RefreshAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        public void ProcessListString(string text, char splitChar) {
            StringUtilities.ProcessStringLists(text, splitChar, _linesToSkip, Importer.ProcessImport);
        }

        [SerializeField]
        private string[] _modifiersPaths = {
            "GameData\\Items\\Mods\\"
        };

        public override void RefreshAssets() {
            base.RefreshAssets();
            _itemModifiers.Clear();
            foreach (var modifiersSearchPath in _modifiersPaths) {
                var folderPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Assets\\" + modifiersSearchPath;
                AddDataInPathGeneric<ItemModifier>(folderPath, "*.asset", _itemModifiers);
            }
        }
        #endif
    }
}
 
