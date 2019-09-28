using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct TakeDamageEvent : IEntityMessage {
        public float Amount { get; }
        public ImpactEvent Impact { get; }
        public CharacterNode Origin { get; }
        public CharacterNode Target { get; }
        public string DamageType { get; }
        public string TargetVital { get; }

        public TakeDamageEvent(float amount, CharacterNode origin, CharacterNode target, string damageType, string targetVital) {
            Impact = default(ImpactEvent);
            Amount = amount;
            Origin = origin;
            Target = target;
            DamageType = damageType;
            TargetVital = targetVital;
        }

        public TakeDamageEvent(float amount, ImpactEvent impact, string damageType, string targetVital) {
            Amount = amount;
            Impact = impact;
            Origin = impact.Origin;
            Target = impact.Target;
            DamageType = damageType;
            TargetVital = targetVital;
        }

        public string ToDescription() {
            return string.Format("{0:F0} {1}", Amount, GameData.DamageTypes.GetNameAt(DamageType));
        }
    }

    public struct CausedDamageEvent : IEntityMessage {
        public float Amount { get; }
        public TakeDamageEvent TakeDamage { get; }

        public CausedDamageEvent(float amount, TakeDamageEvent damageEvent) {
            TakeDamage = damageEvent;
            Amount = amount;
        }
    }
    

    public struct HealingEvent : IEntityMessage {
        public float Amount { get; }
        public Entity Origin { get; }
        public Entity Target { get; }
        public string TargetVital { get; }

        public HealingEvent(float amount, Entity origin, Entity target, string targetVital) {
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
        public CharacterNode Source { get; }
        public CharacterNode Target { get; }

        public RaiseDeadEvent(CharacterNode source, CharacterNode target) {
            Source = source;
            Target = target;
        }
    }
}
