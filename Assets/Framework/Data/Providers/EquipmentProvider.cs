﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class EquipmentProvider : IDataFactory<Equipment> {

        private static GameOptions.CachedString _defaultEquipmentSlot = new GameOptions.CachedString("DefaultEquipmentSlot");

        public void AddComponent(Entity entity, DataEntry data) {
            var equipment = entity.Add(new Equipment(data.TryGetValue(DatabaseFields.EquipmentSlot, _defaultEquipmentSlot.Value)));
            StatExtensions.AddStatList(entity, data.Get<DataList>(DatabaseFields.Stats), equipment);
            var stats = entity.Get<StatsContainer>();
            if (stats.HasStat(Stat.Weight) && !equipment.StatsToEquip.Contains(Stat.Weight)) {
                equipment.StatsToEquip.Add(Stat.Weight);
            }
        }
    }
}
