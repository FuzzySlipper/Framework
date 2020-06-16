using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;


namespace PixelComrades {
    public class ScriptableDatabaseTable {
        [TableList(IsReadOnly = true, AlwaysExpanded = true), ShowInInspector]
        private readonly List<ScriptableObjectWrapper> _allObjs;
        
        public ScriptableObjectWrapper this[int index] { get { return _allObjs[index]; } }

        public ScriptableDatabaseTable(List<ScriptableObjectWrapper> allObjs) {
            _allObjs = allObjs;
        }
    }

    public abstract class ScriptableObjectWrapper {

        public ScriptableDatabase Database { get; }
        public UnityEngine.Object Obj { get; }

        public abstract Texture Icon { get; }

        protected ScriptableObjectWrapper(ScriptableDatabase db, UnityEngine.Object obj) {
            Obj = obj;
            Database = db;
        }
    }

    public class ItemWrapper : ScriptableObjectWrapper {

        private ItemConfig _item;
        private Texture _icon;
        
        public override Texture Icon { get { return _icon; } }

        public ItemWrapper(ScriptableDatabase db, ItemConfig config) : base(db, config) {
            _item = config;
            _icon = config.GetPreviewTexture();
        }

        [TableColumnWidth(120)]
        [ShowInInspector]
        public string Name {
            get { return _item.Name; }
            // set {
            //     _item.Name = value;
            //     EditorUtility.SetDirty(_item);
            // }
        }
        
        [ShowInInspector, ProgressBar(0, 100)]
        public int Price {
            get { return _item.Price; }
            set {
                _item.Price = value;
                EditorUtility.SetDirty(_item);
            }
        }
        
        [ShowInInspector]
        public ItemRarity Rarity {
            get { return _item.Rarity; }
            set {
                _item.Rarity = value;
                EditorUtility.SetDirty(_item);
            }
        }

        private ValueDropdownList<string> SkillSlotList() {
            return Skills.GetDropdownList();
        }

        // [ShowInInspector, ProgressBar(0, 100)]
        // public int Price {
        //     get { return _item.Price; }
        //     set {
        //         _item.Price = value;
        //         EditorUtility.SetDirty(_item);
        //     }
        // }
    }
    public class EquipmentWrapper : ItemWrapper {

        private EquipmentConfig _equipment;

        public EquipmentWrapper(ScriptableDatabase db, EquipmentConfig config) : base(db, config) {
            _equipment = config;
        }

        [ShowInInspector, ValueDropdown("SkillSlotList")]
        public string Skill {
            get {
                return _equipment.Skill;
            }
            set {
                _equipment.Skill = value;
                EditorUtility.SetDirty(_equipment);
            }
        }

        private ValueDropdownList<string> SkillSlotList() {
            return Skills.GetDropdownList();
        }

        [ShowInInspector, ProgressBar(0, 100)]
        public int PhysicalDefense {
            get { return _equipment.DefenseBonuses[0]; }
            set {
                _equipment.DefenseBonuses[0] = value;
                EditorUtility.SetDirty(_equipment);
            }
        }
        // [ShowInInspector, ProgressBar(0, 100)]
        // public int Price {
        //     get { return _item.Price; }
        //     set {
        //         _item.Price = value;
        //         EditorUtility.SetDirty(_item);
        //     }
        // }
    }

    public class WeaponWrapper : ItemWrapper {

        private WeaponConfig _weapon;

        public WeaponWrapper(ScriptableDatabase db, WeaponConfig config) : base(db, config) {
            _weapon = config;
        }

        [ShowInInspector, ValueDropdown("SkillSlotList")]
        public string Skill {
            get {
                return _weapon.Skill;
            }
            set {
                _weapon.Skill = value;
                EditorUtility.SetDirty(_weapon);
            }
        }

        private ValueDropdownList<string> SkillSlotList() {
            return Skills.GetDropdownList();
        }

        [ShowInInspector, ProgressBar(0, 100)]
        public float PowerMin {
            get { return _weapon.Power.Min; }
            set {
                _weapon.Power.Min = value;
                EditorUtility.SetDirty(_weapon);
            }
        }
        [ShowInInspector, ProgressBar(0, 100)]
        public float PowerMax {
            get { return _weapon.Power.Max; }
            set {
                _weapon.Power.Max = value;
                EditorUtility.SetDirty(_weapon);
            }
        }
        
        // [ShowInInspector, ProgressBar(0, 100)]
        // public int Price {
        //     get { return _item.Price; }
        //     set {
        //         _item.Price = value;
        //         EditorUtility.SetDirty(_item);
        //     }
        // }
    }
}
#endif