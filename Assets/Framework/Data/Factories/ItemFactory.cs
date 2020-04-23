using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace PixelComrades {
    public class ItemFactory : ScriptableDatabase<ItemFactory> {
        [ListDrawerSettings(Expanded = true)]
        [SerializeField] private ItemConfig[] _allItems = new ItemConfig[0];

        private static Dictionary<string, List<ItemConfig>> _itemsByType = new Dictionary<string, List<ItemConfig>>();
        private static Dictionary<int, ShuffleBag<ItemConfig>> _maxRarityBags = new Dictionary<int, ShuffleBag<ItemConfig>>();
        private static Dictionary<int, ShuffleBag<ItemConfig>> _specificRarityBags = new Dictionary<int, ShuffleBag<ItemConfig>>();
        private static Dictionary<string, ItemConfig> _items = new Dictionary<string, ItemConfig>();
        public override IEnumerable<UnityEngine.Object> AllObjects { get { return _allItems; } }
        public override Type DbType { get { return typeof(ItemConfig); } }
#if UNITY_EDITOR
        public override System.Object GetEditorWindow() {
            var wrappers = new List<ScriptableObjectWrapper>();
            for (int i = 0; i < _allItems.Length; i++) {
                var item = _allItems[i];
                if (item is EquipmentConfig equipmentConfig) {
                    wrappers.Add(new EquipmentWrapper(this, equipmentConfig));
                }
                else if (item is WeaponConfig weaponConfig) {
                    wrappers.Add(new WeaponWrapper(this, weaponConfig));
                }
                else {
                    wrappers.Add(new ItemWrapper(this, item));
                }
            }
            wrappers.Sort((x, y) => x.GetType().Name.CompareTo(y.GetType().Name));
            return new ScriptableDatabaseTable(wrappers);
        }
#endif
        
        public override void AddObject(Object obj) {
            var item = obj as ItemConfig;
            if (item == null || _allItems.Contains(item)) {
                return;
            }
            _allItems = _allItems.AddToArray(item);
        }

        public override T GetObject<T>(string id) {
            return _items.TryGetValue(id, out var config) ? config as T : null;
        }

        public override string GetId<T>(T obj) {
            return obj is ItemConfig config ? config.ID : "";
        }
        //
        // public class ItemWrapper : ScriptableObjectWrapper {
        //     public ItemConfig Item { get; }
        //     public override Texture Icon { get; }
        //
        //     public ItemWrapper(ScriptableDatabase db, ItemConfig item) : base(db, item) {
        //         Item = item;
        //         Icon = Sirenix.Utilities.Editor.GUIHelper.GetAssetThumbnail(item.Preview, typeof(Sprite), true);
        //     }
        //
        //     [TableColumnWidth(120)]
        //     [ShowInInspector]
        //     public string Name {
        //         get { return Item.Name; }
        //         set {
        //             Item.Name = value;
        //             UnityEditor.EditorUtility.SetDirty(Item);
        //         }
        //     }
        // }

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
  //          for (int i = 0; i < config.GenericComponents.Count; i++) {
                //World.Get<DataFactory>().AddComponentList(entity, config.Data, config.TypeComponents);
//            }
            // ItemModifierFactory.AddModifiers(config.ModifierGroup, level, entity, out DataEntry prefix, out DataEntry suffix);
            StringBuilder sbName = new StringBuilder();
            StringBuilder sbDescr = new StringBuilder();
            sbDescr.Append(config.Description);
            // if (prefix != null) {
            //     var prefixLabel = prefix.TryGetValue(DatabaseFields.Name, "");
            //     if (!string.IsNullOrEmpty(prefixLabel)) {
            //         sbName.Append(prefixLabel);
            //         sbName.Append(" ");
            //     }
            //     var prefixDescr = prefix.TryGetValue(DatabaseFields.Description, "");
            //     if (!string.IsNullOrEmpty(prefixDescr)) {
            //         sbDescr.NewLine();
            //         sbDescr.Append(prefixDescr);
            //     }
            // }
            sbName.Append(config.Name);
            // if (suffix != null) {
            //     var suffixLabel = suffix.TryGetValue(DatabaseFields.Name, "");
            //     if (!string.IsNullOrEmpty(suffixLabel)) {
            //         sbName.Append(" ");
            //         sbName.Append(suffixLabel);
            //     }
            //     var suffixDescr = suffix.TryGetValue(DatabaseFields.Description, "");
            //     if (!string.IsNullOrEmpty(suffixDescr)) {
            //         sbDescr.NewLine();
            //         sbDescr.Append(suffixDescr);
            //     }
            // }
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
