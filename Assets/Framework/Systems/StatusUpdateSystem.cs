using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class StatusUpdateSystem : SystemBase, IReceive<StatusUpdate>, IReceive<CombatStatusUpdate> {

        public StatusUpdateSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(StatusUpdateComponent), typeof(FloatingTextStatusComponent), typeof(FloatingTextCombatComponent),
            }));
        }
        
        public void Handle(StatusUpdate arg) {
#if DEBUG
            DebugLog.Add(arg.Target + " received status " + arg.Update);
#endif
            var statusComponent = arg.Target.Get<StatusUpdateComponent>();
            if (statusComponent != null) {
                statusComponent.Status = arg.Update;
            }
            var floatingText = arg.Target.Find<FloatingTextStatusComponent>();
            if (floatingText != null) {
                FloatingText.Message(arg.Update, floatingText.Tr.position + floatingText.Offset, arg.Color);
            }
        }

        public void Handle(TakeDamageEvent arg) {
            if (!arg.Target.Entity.HasComponent<FloatingText>()) {
                return;
            }
<<<<<<< HEAD
            if (arg.Hit.Result == CollisionResult.CriticalHit) {
=======
            if (arg.Hit == CollisionResult.CriticalHit) {
>>>>>>> FirstPersonAction
                UIFloatingText.WorldSpawn(arg.Amount.ToString("F0") + "!", arg.Target.Tr.position, new Color(1f, 0.6f, 0.14f));
            }
            else {
                UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), arg.Target.Tr.position, Color.red);
            }
            
        }

        public void Handle(HealingEvent arg) {
            if (!arg.Target.Entity.HasComponent<FloatingText>()) {
                return;
            }
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), arg.Target.Entity.GetPosition(), Color.green);
        }

        public void Handle(CombatStatusUpdate arg) {
            var floatingText = arg.Target.Find<FloatingTextCombatComponent>();
            if (floatingText == null) {
                return;
            }
            FloatingText.Message(arg.Update, floatingText.Tr.position + floatingText.Offset, arg.Color);
        }
    }
}
