using UnityEngine;
using System.Collections;
namespace PixelComrades {

    public enum TriggerTargetTypes {
        None = 0,
        Lock = 1,
        Activate = 2,
    }

    public enum LockType {
        None,
        Key,
        Switch,
    }

    public static class TriggerExtensions {

        private static TriggerTargetTypes[] _all;

        public static TriggerTargetTypes[] All {
            get {
                if (_all == null) {
                    _all = new TriggerTargetTypes[EnumHelper.GetLength<TriggerTargetTypes>()];
                    for (int i = 0; i < _all.Length; i++) {
                        _all[i] = (TriggerTargetTypes) i;
                    }
                }
                return _all;
            }
        }

        public static bool AutoActivates(this TriggerTargetTypes triggerType) {
            switch (triggerType) {
                case TriggerTargetTypes.Activate:
                    return true;
            }
            return false;
        }

        public static void BroadcastMessage(this LockType lockType, string id, bool locked) {
            switch (lockType) {
                case LockType.Switch:
                    MessageKit<string>.post(Messages.SwitchToggle, id);
                    break;
                case LockType.Key:
                    MessageKit<string>.post(locked ? Messages.Locked : Messages.Unlocked, id);
                    break;
            }
        }

        public static void AddObserver(this LockType lockType, System.Action<string> delToggle, System.Action<string> delUnlocked, System.Action<string> delLocked) {
            switch (lockType) {
                case LockType.Switch:
                    MessageKit<string>.addObserver(Messages.SwitchToggle, delToggle);
                    break;
                case LockType.Key:
                    MessageKit<string>.addObserver(Messages.Unlocked, delUnlocked);
                    MessageKit<string>.addObserver(Messages.Locked, delLocked);
                    break;
            }
        }

        public static void RemoveObserver(this LockType lockType, System.Action<string> delToggle, System.Action<string> delUnlocked, System.Action<string> delLocked) {
            switch (lockType) {
                case LockType.Switch:
                    MessageKit<string>.removeObserver(Messages.SwitchToggle, delToggle);
                    break;
                case LockType.Key:
                    MessageKit<string>.removeObserver(Messages.Unlocked, delUnlocked);
                    MessageKit<string>.removeObserver(Messages.Locked, delLocked);
                    break;
            }
        }

        public static StaticNoticeMsg FarUnlockedMessage = new StaticNoticeMsg("You hear a click to the {0}");
        public static string FarUnlockedString = "You hear a click to the {0}";

        public static StaticNoticeMsg UnlockedMessage = new StaticNoticeMsg("You unlocked the {0}");
        public static string UnlockedString = "Unlocked!";
    }
}
