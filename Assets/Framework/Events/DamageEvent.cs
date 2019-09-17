using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct DamageEvent : IEntityMessage {
        public float Amount;
        public CharacterNode Origin { get; }
        public CharacterNode Target { get; }
        public string DamageType { get; }
        public string TargetVital { get; }

        public DamageEvent(float amount, CharacterNode origin, CharacterNode target, string damageType, string targetVital) {
            Amount = amount;
            Origin = origin;
            Target = target;
            DamageType = damageType;
            TargetVital = targetVital;
        }

        public string ToDescription() {
            return string.Format("{0:F0} {1}", Amount, GameData.DamageTypes.GetNameAt(DamageType));
        }
    }

    public struct HealEvent : IEntityMessage {
        public float Amount;
        public Entity Origin { get; }
        public Entity Target { get; }
        public string TargetVital { get; }

        public HealEvent(float amount, Entity origin, Entity target, string targetVital) {
            Amount = amount;
            Origin = origin;
            Target = target;
            TargetVital = targetVital;
        }
    }

    public struct DeathEvent : IEntityMessage {
        public CharacterNode Caused { get; }
        public CharacterNode Target { get; }
        public float OverKill { get; }

        public DeathEvent(CharacterNode caused, CharacterNode target, float overKill) {
            Caused = caused;
            Target = target;
            OverKill = overKill;
        }
    }

    public struct RaiseDeadEvent : IEntityMessage {
        public Entity Source { get; }
        public Entity Target { get; }

        public RaiseDeadEvent(Entity source, Entity target) {
            Source = source;
            Target = target;
        }
    }
}
