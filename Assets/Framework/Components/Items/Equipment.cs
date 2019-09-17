using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PixelComrades {
    [System.Serializable]
	public sealed class Equipment : IComponent {
        
        public Equipment(SerializationInfo info, StreamingContext context) {
            EquipmentSlotType = info.GetValue(nameof(EquipmentSlotType), EquipmentSlotType);
            StatsToEquip = info.GetValue(nameof(StatsToEquip), StatsToEquip);
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(EquipmentSlotType), EquipmentSlotType);
            info.AddValue(nameof(StatsToEquip), StatsToEquip);
        }

        public List<string> StatsToEquip = new List<string>();
        public StatModHolder[] Mods;

        public string EquipmentSlotType { get; }

        public Equipment(string equip) {
            EquipmentSlotType = equip;
        }

        public void AddStat(string stat) {
            if (StatsToEquip.Contains(stat)) {
                return;
            }
            StatsToEquip.Add(stat);
        }
        
        public void ClearCurrentMods() {
            if (Mods == null) {
                return;
            }
            for (int i = 0; i < Mods.Length; i++) {
                Mods[i].Remove();
            }
        }

    }
}
