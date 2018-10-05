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

        public StatsContainer Stats { get { return Entity.Stats; } }

        public CharacterNode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Register(entity, list);
        }

        public CharacterNode(){}

        public virtual void Register(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Label.Set(entity, list);
            Dead.Set(entity, list);
            Modifiers.Set(entity, list);
            Status.Set(entity, list);
            Position.Set(entity, list);
            Faction.Set(entity, list);
            Commands.Set(entity, list);
            Slots.Set(entity, list);
        }

        public VitalStat GetVital(int vital) {
            return Entity.Stats.GetVital(GameData.Vitals.GetID(vital));
        }

        public virtual void Dispose() {
            Label.Dispose();
            Dead.Dispose();
            Modifiers.Dispose();
            Status.Dispose();
            Position.Dispose();
            Faction.Dispose();
            Commands.Dispose();
            Slots.Dispose();
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

    public static class CharacterNodeExtensions {
        public static bool IsDead(this CharacterNode node) {
            return node.Entity.Tags.Contain(EntityTags.IsDead);
        }
    }
}
