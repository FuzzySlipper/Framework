using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEquipmentHolder {
        Entity Owner { get; }
        IEntityContainer Container { get; }
        System.Action<Entity> OnItemChanged { get; set; }
        string TargetSlot { get; }
        string LastEquipStatus { get; set; }
        string[] CompatibleSlots { get; }
        System.Type[] RequiredTypes { get; }
        Entity Item { get; set; }
        Transform EquipTr { get; }
        List<StatModHolder> CurrentStats { get; }
        bool FinalCheck(Entity item, out string error);
    }
}
