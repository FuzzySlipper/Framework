using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    public enum ActionCommandType {
        Attack,
        WeaponAttack,
        WeaponAttackAbility,
        WeaponAttackStealth,
        WeaponAttackDeath,
        Heal,
        SetModifier,
        ConvertVital,
        CreateScroll,
        DefendOthers,
        RaiseDead,
        RepairWeaponJam,
        Unlock,
        RemoveMod,
        DamageOverTime,
        AttackWithMods,
        Teleport,
        InstantKill,
    }
}
