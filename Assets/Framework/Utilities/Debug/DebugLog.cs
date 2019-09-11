using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
#if DEBUG
    public static class DebugLog {

        private static StringBuilder _sb = new StringBuilder();
        private static GameOptions.CachedBool _getStackTrace = new GameOptions.CachedBool("LogStackTrace");
        private static GameOptions.CachedBool _disableLog = new GameOptions.CachedBool("DisableLog");

        public static string Current { get { return _sb.ToString(); } }

        public static void Add(string msg, bool addStackTrace = true) {
            if (_disableLog) {
                return;
            }
            _sb.Append(TimeManager.TimeUnscaled.ToString("F2"));
            _sb.Append("-");
            _sb.Append(msg);
            _sb.Append(Environment.NewLine);
            if (!addStackTrace || !_getStackTrace) {
                return;
            }
            _sb.Append(System.Environment.StackTrace);
            _sb.Append(Environment.NewLine);
        }
    }
#endif
}
