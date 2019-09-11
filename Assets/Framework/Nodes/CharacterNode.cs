using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CharacterNode : INode {
        public Entity Entity { get; private set; }

        public CachedComponent<LabelComponent> Label = new CachedComponent<LabelComponent>();
        public CachedComponent<DamageComponent> Dead = new CachedComponent<DamageComponent>();
        public CachedComponent<StatusContainer> Status = new CachedComponent<StatusContainer>();
        public CachedComponent<GridPosition> Position = new CachedComponent<GridPosition>();
        public CachedComponent<FactionComponent> Faction = new CachedComponent<FactionComponent>();
        public CachedComponent<CurrentActions> CurrentActions = new CachedComponent<CurrentActions>();
        public CachedComponent<EquipmentSlots> Slots = new CachedComponent<EquipmentSlots>();
        public CachedComponent<CommandTarget> Target = new CachedComponent<CommandTarget>();
        

        public StatsContainer Stats => Entity.Stats;
        public bool IsDead => Entity.Tags.Contain(EntityTags.IsDead);

        public virtual List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            Label, Dead, Status, Position, Faction, CurrentActions, Slots, Target
        };

        public void Register(Entity entity, SortedList<Type, ComponentReference> list) {
            Entity = entity;
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Set(entity, list);
            }
        }

        public VitalStat GetVital(int vital) {
            return Entity.Stats.GetVital(GameData.Vitals.GetID(vital));
        }

        public void Dispose() {
            var components = GatherComponents;
            for (int i = 0; i < components.Count; i++) {
                components[i].Dispose();
            }
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(LabelComponent),
                typeof(DamageComponent),
                typeof(GridPosition),
                typeof(FactionComponent),
                typeof(CurrentActions),
            };
        }
    }
}
