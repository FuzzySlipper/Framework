using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public enum ModifierGroups {
        None = 0,
        Melee,
        ScienceMelee,
        MagicMelee,
        Range,
        ScienceRange,
        MagicRange,
        SimpleOutfit,
        ScienceOutfit,
        MagicOutfit,
        HolyOutfit,
        Armor,
        ScienceArmor,
        MagicUsable,
        ScienceUsable,
        HolyUsable,
    }

    public abstract class ItemModifier : ScriptableObject {

        [SerializeField] private int _minRarity = 0;
        [SerializeField, Range(1, 100)] private float _chance = 5;
        [SerializeField] private List<ModifierGroups> _modGroups = new List<ModifierGroups>();
        [SerializeField] private string _descriptiveName = "";
        [SerializeField] private bool _isPrefix = true;
        [SerializeField] private int _minLevel = 1;
        [SerializeField] private bool _isMagic = false;

        public bool IsMagic { get { return _isMagic; } }
        public int MinLevel { get { return _minLevel; } }
        public int MinRarity { get { return _minRarity; } }
        public float Chance { get { return _chance; } }
        public List<ModifierGroups> ValidTypes { get { return _modGroups; } }
        public string DescriptiveName { get { return _descriptiveName; } }
        public bool IsPrefix { get { return _isPrefix; } }
        public string Id { get { return name; } }

        public abstract void Init(int level, Entity item);

    }
}
