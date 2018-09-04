using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades{
    public interface ILevelTriggerReceiver {
        bool IsActive { get; set; }
        TriggerTargetTypes TriggerType { get; set; }
        KeyHole Keyhole { get; }
        void LevelTrigger(ILevelTrigger origin);
    }

    public static class LevelTriggerReceiverExtensions {
        public static void Serialize(this ILevelTriggerReceiver lvlTrigger, ref System.Text.StringBuilder sb) {
            sb.AppendChildEntryBreak(lvlTrigger.IsActive.ToString());
        }
    }

    public enum LevelTriggerEvents {
        Activate,
        Deactivate,
        Locked,
        Unlocked
    }
}
