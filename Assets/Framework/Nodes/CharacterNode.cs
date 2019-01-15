using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CharacterNode : INode {
        public Entity Entity { get; private set; }

        public CachedComponent<LabelComponent> Label = new CachedComponent<LabelComponent>();
        public CachedComponent<DeathStatus> Dead = new CachedComponent<DeathStatus>();
        public CachedComponent<ModifiersContainer> Modifiers = new CachedComponent<ModifiersContainer>();
        public CachedComponent<StatusContainer> Status = new CachedComponent<StatusContainer>();
        public CachedComponent<GridPosition> Position = new CachedComponent<GridPosition>();
        public CachedComponent<FactionComponent> Faction = new CachedComponent<FactionComponent>();
        public CachedComponent<CommandsContainer> Commands = new CachedComponent<CommandsContainer>();
        public CachedComponent<EquipmentSlots> Slots = new CachedComponent<EquipmentSlots>();

        public StatsContainer Stats => Entity.Stats;
        public bool IsDead => Entity.Tags.Contain(EntityTags.IsDead);

        public virtual List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            Label, Dead, Modifiers, Status, Position, Faction, Commands, Slots
        };

        public void Register(Entity entity, Dictionary<System.Type, ComponentReference> list) {
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
                typeof(DeathStatus),
                typeof(ModifiersContainer),
                typeof(StatusContainer),
                typeof(GridPosition),
                typeof(FactionComponent),
                typeof(CommandsContainer),
            };
        }
    }
}
