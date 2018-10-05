using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EquipmentProvider : IDataFactory<Equipment> {

        public void AddComponent(Entity entity, DataEntry data) {
            var equipment = entity.Add(new Equipment(data.GetEnum(DatabaseFields.EquipmentSlot, 0)));
            StatExtensions.AddStatList(entity, data.Get<DataList>(DatabaseFields.Stats), equipment);
            if (entity.Stats.HasStat(Stats.Weight)) {
                entity.Get<Equipment>(e => e.AddStat(Stats.Weight));
            }
        }
    }
}
