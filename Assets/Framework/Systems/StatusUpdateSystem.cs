using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class StatusUpdateSystem : SystemBase, IReceive<StatusUpdate>, IReceive<CombatStatusUpdate> {

        public StatusUpdateSystem() {
            EntityController.RegisterReceiver<StatusUpdateComponent>(this);
            EntityController.RegisterReceiver<FloatingTextStatusComponent>(this);
            EntityController.RegisterReceiver<FloatingTextCombatComponent>(this);
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

        public void Handle(DamageEvent arg) {
            if (!arg.Target.Entity.HasComponent<FloatingText>()) {
                return;
            }
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), arg.Target.Tr.position, Color.red);
        }

        public void Handle(HealEvent arg) {
            if (!arg.Target.HasComponent<FloatingText>()) {
                return;
            }
            UIFloatingText.WorldSpawn(arg.Amount.ToString("F0"), arg.Target.GetPosition(), Color.green);
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
