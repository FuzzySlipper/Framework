using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public static class ItemFactory  {

        private static Dictionary<int, ShuffleBag<DataEntry>> _maxRarityBags = new Dictionary<int, ShuffleBag<DataEntry>>();
        private static Dictionary<int, ShuffleBag<DataEntry>> _specificRarityBags = new Dictionary<int, ShuffleBag<DataEntry>>();
        private static Dictionary<string, DataEntry> _items = new Dictionary<string, DataEntry>();
        private static Dictionary<string, DataEntry> _itemsFullID = new Dictionary<string, DataEntry>();

        private static void Init() {
            GameData.AddInit(Init);
            _maxRarityBags.Clear();
            _specificRarityBags.Clear();
            _items.Clear();
            _itemsFullID.Clear();
            for (int s = 0; s < DatabaseSheets.ItemSheets.Length; s++) {
                var sheet = GameData.GetSheet(DatabaseSheets.ItemSheets[s]);
                if (sheet == null) {
                    continue;
                }
                foreach (var loadedDataEntry in sheet) {
                    var item = loadedDataEntry.Value;
                    _items.SafeAdd(item.ID, item);
                    _itemsFullID.SafeAdd(item.FullID, item);
                    if (!item.TryGetEnum(StatTypes.ItemRarity, out var bagIndex)) {
                        continue;
                    }
                    var chance = (int) item.GetValue<float>(DatabaseFields.Chance) * 100;
                    AddToShuffleBag(_specificRarityBags, item, chance, bagIndex);
                    for (int i = 0; i <= bagIndex; i++) {
                        AddToShuffleBag(_maxRarityBags, item, chance, bagIndex);
                    }
                }
            }
        }

        public static Entity GetItem(string itemID, int level = 1) {
            var data = FindItem(itemID);
            if (data == null) {
                return null;
            }
            return CreateItem(data, level);
        }

        public static Entity RandomItem(int rarity, bool isMaxRare = true) {
            if (_items.Count == 0) {
                Init();
            }
            var shuffleBag = GetRarity(rarity, isMaxRare);
            if (shuffleBag != null) {
                return CreateItem(shuffleBag.Next(), ItemRandomCurrentLevel());
            }
            return null;
        }

        public static int ItemRandomCurrentLevel() {
            var max = Mathf.Clamp(Player.HighestCurrentLevel, 2, World.Get<MapSystem>().Level);
            var range = GameOptions.Get(RpgSettings.ItemRandomLevelRange, 1);
            return Mathf.Clamp(Game.Random.Next(max - range, max + range), 2, 99);
        }

        public static DataEntry RandomTemplate(int rarity, bool isMaxRare = true) {
            if (_items.Count == 0) {
                Init();
            }
            var shuffleBag = GetRarity(rarity, isMaxRare);
            if (shuffleBag != null) {
                return shuffleBag.Next();
            }
            return null;
        }

        private static Entity CreateItem(DataEntry data, int level) {
            var entity = Entity.New(data.ID);
            entity.Add(new TypeId(data.ID));
            var icon = data.GetValue<string>(DatabaseFields.Icon);
            if (!string.IsNullOrEmpty(icon)) {
                entity.Add(new IconComponent(ItemPool.LoadAsset<Sprite>(UnityDirs.Icons, icon)));
            }
            else {
                entity.Add(new IconComponent(SpriteDatabase.Item));
            }
            entity.Add(new EntityLevelComponent(level));
            var itemTypeData = data.Get<DataReference>(DatabaseFields.ItemType);
            if (itemTypeData?.Value != null) {
                World.Get<DataFactory>().AddComponentList(entity, data, itemTypeData.Value.Get(DatabaseFields.Components) as DataList);
            }
            World.Get<DataFactory>().AddComponentList(entity, data, data.Get(DatabaseFields.Components) as DataList);

            if (data.TryGetEnum(DatabaseFields.ModifierGroup, out var modGroup)) {
                ItemModifierFactory.AddModifiers(modGroup, level, entity);
            }
            StringBuilder sbName = new StringBuilder();
            StringBuilder sbDescr = new StringBuilder();
            sbDescr.Append(data.TryGetValue(DatabaseFields.Description, data.ID));
            var prefix = entity.Get<ItemModifierPrefix>();
            var suffix = entity.Get<ItemModifierSuffix>();
            if (prefix != null) {
                var prefixLabel = prefix.Data.TryGetValue(DatabaseFields.Name, "");
                if (!string.IsNullOrEmpty(prefixLabel)) {
                    sbName.Append(prefixLabel);
                    sbName.Append(" ");
                }
                var prefixDescr = prefix.Data.TryGetValue(DatabaseFields.Description, "");
                if (!string.IsNullOrEmpty(prefixDescr)) {
                    sbDescr.NewLine();
                    sbDescr.Append(prefixDescr);
                }
            }

            sbName.Append(data.TryGetValue(DatabaseFields.Name, data.ID));
            if (suffix != null) {
                var suffixLabel = suffix.Data.TryGetValue(DatabaseFields.Name, "");
                if (!string.IsNullOrEmpty(suffixLabel)) {
                    sbName.Append(" ");
                    sbName.Append(suffixLabel);
                }
                var suffixDescr = suffix.Data.TryGetValue(DatabaseFields.Description, "");
                if (!string.IsNullOrEmpty(suffixDescr)) {
                    sbDescr.NewLine();
                    sbDescr.Append(suffixDescr);
                }
            }
            entity.Add(new LabelComponent(sbName.ToString()));
            entity.Add(new DescriptionComponent(sbDescr.ToString()));
            return entity;
        }

        private static ShuffleBag<DataEntry> GetRarity(int rarity, bool isMaxRare) {
            int currentRarity = (int) rarity;
            var rarityCollection = isMaxRare ? _maxRarityBags : _specificRarityBags;
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

        private static void AddToShuffleBag(Dictionary<int, ShuffleBag<DataEntry>> dict, DataEntry item, int chance, int bagIndex) {
            if (!dict.TryGetValue(bagIndex, out var bag)) {
                bag = new ShuffleBag<DataEntry>();
                dict.Add(bagIndex, bag);
            }
            bag.Add(item, chance);
        }

        private static DataEntry FindItem(string itemID) {
            if (_items.Count == 0) {
                Init();
            }
            if (_items.TryGetValue(itemID, out var value)) {
                return value;
            }
            return _itemsFullID.TryGetValue(itemID, out value) ? value : null;
        }
    }
}
