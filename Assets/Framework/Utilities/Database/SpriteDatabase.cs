using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class SpriteDatabase : ScriptableSingleton<SpriteDatabase> {

        [SerializeField] private Sprite _attackIcon = null;
        [SerializeField] private Sprite _rangeAttackIcon = null;
        [SerializeField] private Sprite _unarmedIcon = null;
        [SerializeField] private Sprite _spellIcon = null;
        [SerializeField] private Sprite _abilityIcon = null;
        [SerializeField] private Sprite _defendIcon = null;
        [SerializeField] private Sprite _itemsIcon = null;
        [SerializeField] private Sprite _move = null;
        [SerializeField] private Sprite _idle = null;
        [SerializeField] private Sprite _dead = null;
        [SerializeField] private ActorPortrait[] _portraits = new ActorPortrait[1];
        [SerializeField] private Sprite[] _spellSkills = new Sprite[4];
        [SerializeField] private Sprite _defaultWeaponIcon = null;
        [SerializeField] private Sprite _defaultUsableIcon = null;
        [SerializeField] private Sprite _defaultShieldIcon = null;
        [SerializeField] private Sprite _defaultArmorIcon = null;
        [SerializeField] private Sprite _defaultHelmetIcon = null;
        [SerializeField] private Sprite _defaultBootsIcon = null;
        [SerializeField] private Sprite _defaultGlovesIcon = null;
        [SerializeField] private Sprite _defaultLegsIcon = null;
        [SerializeField] private Sprite[] _currencyIcons = new Sprite[0];

        public static Sprite DefaultArmorIcon { get { return Main._defaultArmorIcon; } }
        public static Sprite DefaultHelmetIcon { get { return Main._defaultHelmetIcon; } }
        public static Sprite DefaultBootsIcon { get { return Main._defaultBootsIcon; } }
        public static Sprite DefaultGlovesIcon { get { return Main._defaultGlovesIcon; } }
        public static Sprite DefaultLegsIcon { get { return Main._defaultLegsIcon; } }
        public static Sprite DefaultWeaponIcon { get { return Main._defaultWeaponIcon; } }
        public static Sprite DefaultShieldIcon { get { return Main._defaultShieldIcon; } }
        public static Sprite DefaultUsableItem { get { return Main._defaultUsableIcon; } }
        public static Sprite Attack { get { return Main._attackIcon; } }
        public static Sprite RangedAttack { get { return Main._rangeAttackIcon; } }
        public static Sprite Unarmed { get { return Main._unarmedIcon; } }
        public static Sprite Spell { get { return Main._spellIcon; } }
        public static Sprite Ability { get { return Main._abilityIcon; } }
        public static Sprite Defend { get { return Main._defendIcon; } }
        public static Sprite Item { get { return Main._itemsIcon; } }
        public static Sprite Move { get { return Main._move; } }
        public static Sprite Idle { get { return Main._idle; } }
        public static Sprite Dead { get { return Main._dead; } }
        public static Sprite[] CurrencyIcons { get { return Main._currencyIcons; } }
        public static Sprite[] SpellSkills { get { return Main._spellSkills; } }
        public static ActorPortrait[] Portraits { get { return Main._portraits; } }

        public static ActorPortrait GetActorSprite(string fullBodyName) {
            for (int i = 0; i < Portraits.Length; i++) {
                if (Portraits[i].FullBody.name.CompareCaseInsensitive(fullBodyName)) {
                    return Portraits[i];
                }
            }
            return null;
        }
    }

    [Serializable]
    public class ActorPortrait {
        public Sprite FullBody;
        public Sprite Head;
        public PlayerPronouns DefaultPronoun;
    }
}
