using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public class ItemFactory : ScriptableSingleton<ItemFactory> {
        
        [SerializeField] private ItemConfig[] _allItems = new ItemConfig[0];

        private static Dictionary<string, List<ItemConfig>> _itemsByType = new Dictionary<string, List<ItemConfig>>();
        private static Dictionary<int, ShuffleBag<ItemConfig>> _maxRarityBags = new Dictionary<int, ShuffleBag<ItemConfig>>();
        private static Dictionary<int, ShuffleBag<ItemConfig>> _specificRarityBags = new Dictionary<int, ShuffleBag<ItemConfig>>();
        private static Dictionary<string, ItemConfig> _items = new Dictionary<string, ItemConfig>();

        private static void Init() {
            GameData.AddInit(Init);
            _maxRarityBags.Clear();
            _specificRarityBags.Clear();
            _items.Clear();
            for (int i = 0; i < Main._allItems.Length; i++) {
                var item = Main._allItems[i];
                var itemTypeData = item.ItemType;
                if (itemTypeData != null) {
                    if (!_itemsByType.TryGetValue(itemTypeData, out var list)) {
                        list = new List<ItemConfig>();
                        _itemsByType.Add(itemTypeData, list);
                    }
                    list.Add(item);
                }
                _items.AddOrUpdate(item.ID, item);
                var bagIndex = (int)item.Rarity;
                var chance = (EnumHelper.GetLength<ItemRarity>() + 1 - bagIndex) * 100;
                AddToShuffleBag(_specificRarityBags, item, chance, bagIndex);
                for (int s = 0; s <= bagIndex; s++) {
                    AddToShuffleBag(_maxRarityBags, item, chance, bagIndex);
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

        public static ItemConfig GetData(string itemID) {
            return FindItem(itemID);
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

        public static ItemConfig RandomTemplate(ItemRarity rarity, bool isMaxRare = true) {
            if (_items.Count == 0) {
                Init();
            }
            var shuffleBag = GetRarity((int) rarity, isMaxRare);
            if (shuffleBag != null) {
                return shuffleBag.Next();
            }
            return null;
        }

        public static Entity CreateItem(ItemConfig config, int level) {
            var entity = Entity.New(config.ID);
            entity.Add(new TypeId(config.ID));
            entity.Add(new StatsContainer());
            if (config.Icon.Asset != null) {
                entity.Add(new IconComponent((Sprite)config.Icon.Asset, ""));
            }
            else {
                config.Icon.LoadAssetAsync().Completed += handle => entity.Add(new IconComponent(handle.Result, ""));
            }
            entity.Add(new EntityLevelComponent(level));
            entity.Add(new TooltipComponent());
            entity.Add(new StatusUpdateComponent());
            config.AddComponents(entity);
            var dataDescr = entity.Add(new DataDescriptionComponent());
            for (int i = 0; i < config.GenericComponents.Count; i++) {
                //World.Get<DataFactory>().AddComponentList(entity, config.Data, config.TypeComponents);
            }
            ItemModifierFactory.AddModifiers(config.ModifierGroup, level, entity, out DataEntry prefix, out DataEntry suffix);
            StringBuilder sbName = new StringBuilder();
            StringBuilder sbDescr = new StringBuilder();
            sbDescr.Append(config.Description);
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

            sbName.Append(config.Name);
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

        private static ShuffleBag<ItemConfig> GetRarity(int rarity, bool isMaxRare) {
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

        private static void AddToShuffleBag(Dictionary<int, ShuffleBag<ItemConfig>> dict, ItemConfig item, int chance, int bagIndex) {
            if (!dict.TryGetValue(bagIndex, out var bag)) {
                bag = new ShuffleBag<ItemConfig>();
                dict.Add(bagIndex, bag);
            }
            bag.Add(item, chance);
        }

        private static ItemConfig FindItem(string itemID) {
            if (_items.Count == 0) {
                Init();
            }
            if (_items.TryGetValue(itemID, out var value)) {
                return value;
            }
            return null;
        }
    }

    
}
