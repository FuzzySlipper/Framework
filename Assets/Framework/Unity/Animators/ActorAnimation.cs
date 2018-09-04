using System.Collections.Generic;
using UnityEngine;

namespace PixelComrades {
    public enum ActorAnimations {
        Move = 0,
        GetHit = 1,
        Action = 2,
        SpecialAction = 3,
        Death = 4,
        Idle = 5,
    }

    public static class ActorAnimation {

        public const string Move = "Move";
        public const string GetHit = "GetHit";
        public const string Action = "Action";
        public const string SpecialAction = "SpecialAction";
        public const string Death = "Death";
        public const string Idle = "Idle";

        public const string Unknown = "Unknown";

        public const int Length = 6;

        public static string[] AllAnimations = {
            Move, GetHit, Action, SpecialAction, Death, Idle
        };

        public static bool CanOverride(this ActorAnimations anim) {
            switch (anim) {
                case ActorAnimations.Idle:
                case ActorAnimations.Move:
                    return true;
            }
            return false;
        }

        private static Dictionary<int, int> _hashToIndex;
        private static bool _hashSet;

        public static class EventParser {
            public static readonly char MESSAGE_DELIMITER = '\t';
            public static readonly string MESSAGE_NOPARAM = "_Anim";
            public static readonly string MESSAGE_INT = "_AnimInt";
            public static readonly string MESSAGE_FLOAT = "_AnimFloat";
            public static readonly string MESSAGE_STRING = "_AnimString";
            public static readonly string MESSAGE_OBJECT_FUNCNAME = "_AnimObjectFunc";
            public static readonly string MESSAGE_OBJECT_DATA = "_AnimObjectData";

            /// Parses value from the passed messageString, and modifies it to just contain the message function name
            public static float ParseFloat(ref string messageString) {
                // Data is in form "<functionname>\t<float>"
                var splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
                float result = 0;
                float.TryParse(messageString.Substring(splitAt + 1), out result);
                messageString = messageString.Substring(0, splitAt);
                return result;
            }

            /// Parses value from the passed messageString, and modifies it to just contain the message function name
            public static int ParseInt(ref string messageString) {
                // Data is in form "<functionname>\t<int>"
                var splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
                var result = 0;
                int.TryParse(messageString.Substring(splitAt + 1), out result);
                messageString = messageString.Substring(0, splitAt);
                return result;
            }

            /// Parses value from the passed messageString, and modifies it to just contain the message function name
            public static string ParseString(ref string messageString) {
                // Data is in form "<functionname>\t<string>"
                var splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
                var result = messageString.Substring(splitAt + 1);
                messageString = messageString.Substring(0, splitAt);
                return result;
            }
        }

        private static void SetHash() {
            _hashSet = true;
            _hashToIndex = new Dictionary<int, int>();
            for (var i = 0; i < AllAnimations.Length; i++) {
                var key = Animator.StringToHash(AllAnimations[i]);
                _hashToIndex.Add(key, i);
            }
        }

        public static string GetClip(int animHash) {
            if (!_hashSet) {
                SetHash();
            }
            int index;
            if (_hashToIndex.TryGetValue(animHash, out index)) {
                return AllAnimations[index];
            }
            return Unknown;
        }
    }
}