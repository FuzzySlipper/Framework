using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public static class ItemFactory  {

        private static Dictionary<string, List<ItemTemplate>> _itemsByType = new Dictionary<string, List<ItemTemplate>>();
        private static Dictionary<int, ShuffleBag<ItemTemplate>> _maxRarityBags = new Dictionary<int, ShuffleBag<ItemTemplate>>();
        private static Dictionary<int, ShuffleBag<ItemTemplate>> _specificRarityBags = new Dictionary<int, ShuffleBag<ItemTemplate>>();
        private static Dictionary<string, ItemTemplate> _items = new Dictionary<string, ItemTemplate>();
        private static Dictionary<string, ItemTemplate> _itemsFullID = new Dictionary<string, ItemTemplate>();
        private static Dictionary<string, List<ItemTemplate>> _outfits = new Dictionary<string, List<ItemTemplate>>();

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
                    var outfitList = item.Get<DataList>("OutfitTypes");
                    if (outfitList != null && outfitList.Count > 0) {
                        CreateOutfit(outfitList, item);
                        continue;
                    }
                    AddItem(new ItemTemplate(item));
                }
            }
        }

        private static void CreateOutfit(DataList outfitList, DataEntry originalData) {
            var outfitID = originalData.ID;
            var list = new List<ItemTemplate>();
            var originName = originalData.TryGetValue(DatabaseFields.Name, originalData.ID);
            for (int i = 0; i < outfitList.Count; i++) {
                var line = outfitList[i];
                var slot = line.TryGetValue(DatabaseFields.EquipmentSlot, "Body");
                var weight = line.TryGetValue(DatabaseFields.Weight, 0f);
                var name = line.TryGetValue(DatabaseFields.Name, originalData.ID);
                var iconName  = string.Format("{0}_{1}", originalData.GetValue<string>(DatabaseFields.Icon), slot);
                var newData = originalData.Clone(string.Format("{0}_{1}", originName, slot));
                newData.Replace(DatabaseFields.EquipmentSlot, new DataCell<string>(DatabaseFields.EquipmentSlot, newData, slot));
                newData.Replace(DatabaseFields.Icon, new DataCell<string>(DatabaseFields.Icon, newData, iconName));
                newData.Replace(DatabaseFields.Name, new DataCell<string>(DatabaseFields.Name, newData, string.Format("{0} {1}", originName, name)));
                newData.Replace(DatabaseFields.Price, new DataCell<int>(DatabaseFields.Price, newData, (int) (originalData.TryGetValue(DatabaseFields.Price, 1) * weight)));
                newData.Replace(DatabaseFields.Weight, new DataCell<int>(DatabaseFields.Weight, newData, (int) (originalData.TryGetValue(DatabaseFields.Weight, 1) * weight)));
                var stats = newData.Get<DataList>(DatabaseFields.Stats);
                for (int s = 0; s < stats.Count; s++) {
                    int amount = (int) (stats[s].TryGetValue(DatabaseFields.Amount, 0) * weight);
                    float multi = stats[s].TryGetValue(DatabaseFields.Multiplier, 0f) * weight;
                    stats[s].Replace(DatabaseFields.Amount, new DataCell<int>(DatabaseFields.Amount, stats[s], amount));
                    stats[s].Replace(DatabaseFields.Multiplier, new DataCell<float>(DatabaseFields.Multiplier, stats[s], multi));
                }
                var config = new ItemTemplate(newData);
                AddItem(config);
                list.Add(config);
            }
            _outfits.AddOrUpdate(outfitID, list);
        }


        public static void AddItem(ItemTemplate item) {
            var itemTypeData = item.ItemType;
            if (itemTypeData != null) {
                if (!_itemsByType.TryGetValue(itemTypeData, out var list)) {
                    list = new List<ItemTemplate>();
                    _itemsByType.Add(itemTypeData, list);
                }
                list.Add(item);
            }
            _items.AddOrUpdate(item.ID, item);
            _itemsFullID.AddOrUpdate(item.FullID, item);
            var bagIndex = item.Rarity;
            var chance = ((GameData.Enums[EnumTypes.ItemRarity].Count + 1) - bagIndex) * 100;
            AddToShuffleBag(_specificRarityBags, item, chance, bagIndex);
            for (int i = 0; i <= bagIndex; i++) {
                AddToShuffleBag(_maxRarityBags, item, chance, bagIndex);
            }
        }

        public static Entity GetItem(string itemID, int level = 1) {
            var data = FindItem(itemID);
            if (data == null) {
                return null;
            }
            return CreateItem(data, level);
        }

        public static ItemTemplate GetData(string itemID) {
            return FindItem(itemID);
        }

        private static List<ItemTemplate> GetOutfitList(string outfit) {
            if (_outfits.TryGetValue(outfit, out var list)) {
                return list;
            }
            var split = outfit.Split('.');
            if (split.Length > 1 && _outfits.TryGetValue(split[1], out list)) {
                return list;
            }
            if (_outfits.TryGetValue("Equipment." + outfit, out list)) {
                return list;
            }
            if (_outfits.TryGetValue("Item." + outfit, out list)) {
                return list;
            }
            return null;
        }

        public static void EquipOutfit(Entity entity, string outfit) {
            var equip = entity.Get<EquipmentSlots>();
            var list = GetOutfitList(outfit);
            if (equip == null || list == null) {
                return;
            }
            for (int i = 0; i < list.Count; i++) {
                var item = CreateItem(list[i],1);
                if (item != null) {
                    if (!equip.TryEquip(item)) {
                        Player.MainInventory.Add(item);
                    }
                }
            }
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
            var max = Mathf.Max(Player.HighestCurrentLevel, 2);
            var range = GameOptions.Get(RpgSettings.ItemRandomLevelRange, 1);
            return Mathf.Clamp(Game.Random.Next(max - range, max + range), 2, 99);
        }

        public static ItemTemplate RandomTemplate(int rarity, bool isMaxRare = true) {
            if (_items.Count == 0) {
                Init();
            }
            var shuffleBag = GetRarity(rarity, isMaxRare);
            if (shuffleBag != null) {
                return shuffleBag.Next();
            }
            return null;
        }

        public static Entity CreateItem(ItemTemplate data, int level) {
            var entity = Entity.New(data.ID);
            entity.Add(new TypeId(data.ID));
            entity.Add(new StatsContainer());
            if (!string.IsNullOrEmpty(data.Icon)) {
                entity.Add(new IconComponent(UnityDirs.ItemIcons, data.Icon));
            }
            entity.Add(new EntityLevelComponent(level));
            entity.Add(new TooltipComponent());
            entity.Add(new StatusUpdateComponent());
            var dataDescr = entity.Add(new DataDescriptionComponent());
            if (data.TypeComponents != null) {
                World.Get<DataFactory>().AddComponentList(entity, data.Data, data.TypeComponents);
            }
            if (data.Components != null) {
                World.Get<DataFactory>().AddComponentList(entity, data.Data, data.Components);
            }
            ItemModifierFactory.AddModifiers(data.ModifierGroup, level, entity, out DataEntry prefix, out DataEntry suffix);
            StringBuilder sbName = new StringBuilder();
            StringBuilder sbDescr = new StringBuilder();
            sbDescr.Append(data.Description);
            if (prefix != null) {
                var prefixLabel = prefix.TryGetValue(DatabaseFields.Name, "");
                if (!string.IsNullOrEmpty(prefixLabel)) {
                    sbName.Append(prefixLabel);
                    sbName.Append(" ");
                }
                var prefixDescr = prefix.TryGetValue(DatabaseFields.Description, "");
                if (!string.IsNullOrEmpty(prefixDescr)) {
                    sbDescr.NewLine();
                    sbDescr.Append(prefixDescr);
                }
            }

            sbName.Append(data.Name);
            if (suffix != null) {
                var suffixLabel = suffix.TryGetValue(DatabaseFields.Name, "");
                if (!string.IsNullOrEmpty(suffixLabel)) {
                    sbName.Append(" ");
                    sbName.Append(suffixLabel);
                }
                var suffixDescr = suffix.TryGetValue(DatabaseFields.Description, "");
                if (!string.IsNullOrEmpty(suffixDescr)) {
                    sbDescr.NewLine();
                    sbDescr.Append(suffixDescr);
                }
            }
            entity.Add(new LabelComponent(sbName.ToString()));
            entity.Add(new DescriptionComponent(sbDescr.ToString()));
            entity.Post(new DataDescriptionUpdating(dataDescr));
            return entity;
        }

        private static ShuffleBag<ItemTemplate> GetRarity(int rarity, bool isMaxRare) {
            int currentRarity = (int) rarity;
            var rarityCollection = isMaxRare ? _maxRarityBags : _specificRarityBags;
            while (true) {
                if (rarityCollection.TryGetValue(currentRarity, out var bag) && bag.Count > 0) {
                    return bag;
                }
                currentRarity -= 1;
                if (currentRarity < 0) {
                    return null;
                }
            }
        }

        private static void AddToShuffleBag(Dictionary<int, ShuffleBag<ItemTemplate>> dict, ItemTemplate item, int chance, int bagIndex) {
            if (!dict.TryGetValue(bagIndex, out var bag)) {
                bag = new ShuffleBag<ItemTemplate>();
                dict.Add(bagIndex, bag);
            }
            bag.Add(item, chance);
        }

        private static ItemTemplate FindItem(string itemID) {
            if (_items.Count == 0) {
                Init();
            }
            if (_items.TryGetValue(itemID, out var value)) {
                return value;
            }
            return _itemsFullID.TryGetValue(itemID, out value) ? value : null;
        }
    }

    public class ItemTemplate {

        public DataEntry Data { get;}
        public string Name { get; }
        public string ID { get; }
        public string FullID { get; }
        public string ItemType { get; }
        public string Icon { get; }
        public string ModifierGroup { get; }
        public string Description { get; }
        public int Rarity { get; }
        public DataList TypeComponents { get; }
        public DataList Components { get; }

        public ItemTemplate(DataEntry data) {
            Data = data;
            ID = data.ID;
            FullID = data.FullID;
            var itemType = data.Get<DataReference>(DatabaseFields.ItemType);
            ItemType = itemType?.Value.GetValue<string>(DatabaseFields.Name) ?? "Item";
            var prefix = itemType?.Value.TryGetValue("Prefix", "I_");
            Icon = prefix + data.GetValue<string>(DatabaseFields.Icon);
            TypeComponents = itemType?.Value.Get(DatabaseFields.Components) as DataList;
            Components = data.Get(DatabaseFields.Components) as DataList;
            ModifierGroup = data.GetValue<string>(DatabaseFields.ModifierGroup);
            StringBuilder sbDescr = new StringBuilder();
            sbDescr.Append(data.TryGetValue(DatabaseFields.Description, data.ID));
            Name = data.TryGetValue(DatabaseFields.Name, data.ID);
            Description = data.TryGetValue(DatabaseFields.Description, "");
            Rarity = data.GetEnum(DatabaseFields.Rarity, 0);
        }
    }
}
