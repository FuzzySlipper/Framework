using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class CharacterNode : INode {
        public Entity Entity;

        public CachedComponent<LabelComponent> Label;
        public CachedComponent<GenericStats> Stats;
        public CachedComponent<VitalStats> Vitals;
        public CachedComponent<DeathStatus> Dead;
        public CachedComponent<ModifiersContainer> Modifiers;
        public CachedComponent<StatusContainer> Status;
        public CachedComponent<GridPosition> Position;
        public CachedComponent<FactionComponent> Faction;
        public CachedComponent<CommandsContainer> Commands;


        public CharacterNode(Entity entity, Dictionary<System.Type, ComponentReference> list) {
            Entity = entity;
            Label = new CachedComponent<LabelComponent>(entity, list);
            Stats = new CachedComponent<GenericStats>(entity, list);
            Vitals = new CachedComponent<VitalStats>(entity, list);
            Dead = new CachedComponent<DeathStatus>(entity, list);
            Modifiers = new CachedComponent<ModifiersContainer>(entity, list);
            Status = new CachedComponent<StatusContainer>(entity, list);
            Position = new CachedComponent<GridPosition>(entity, list);
            Faction = new CachedComponent<FactionComponent>(entity, list);
            Commands = new CachedComponent<CommandsContainer>(entity, list);
        }


        public virtual void Dispose() {
            Label.Dispose();
            Stats.Dispose();
            Dead.Dispose();
            Modifiers.Dispose();
            Status.Dispose();
            Position.Dispose();
            Faction.Dispose();
            Vitals.Dispose();
            Label = null;
            Stats = null;
            Vitals = null;
            Dead = null;
            Modifiers = null;
            Status = null;
            Position = null;
            Faction = null;
            Commands.Dispose();
            Commands = null;
        }

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(LabelComponent),
                typeof(GenericStats),
                typeof(VitalStats),
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
