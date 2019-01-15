using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IEntityModifier {
        string Label { get; }
        string Description { get; }
        string Id { get; }
        int TurnStart { get; }
        int TurnLength { get; }
        EntityModCategories Category { get; }
        Entity Owner { get; }
        Entity Target { get; }
        Sprite Icon { get; }
        IEntityModifier Clone();
        bool ShouldRemove();
        void OnAttach(Entity owner, Entity target);
        void OnUpdate();
        void OnRemove();
    }

    public enum EntityModCategories {
        Buff,
        DeBuff,
        StatusDeBuff,
        Healing,
        Damage,
    }

    public struct ModifiersChanged : IEntityMessage {
        public Entity Target;

        public ModifiersChanged(Entity target) {
            Target = target;
        }
    }

    public static class IEntityModifierExtensions {
        public static int TurnsLeft(this IEntityModifier mod) {
            return (mod.TurnStart + mod.TurnLength) - TurnBased.TurnNumber;
        }

        public static int TurnEnd(this IEntityModifier mod) {
            return (mod.TurnStart + mod.TurnLength);
        }
}
}
