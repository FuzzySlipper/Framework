using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public enum UIRoot {
        Player,
        Debug,
        Edge,
        Misc,
    }

    public static class Root {
        private static Dictionary<UIRoot, Canvas> _canvases = new Dictionary<UIRoot, Canvas>();

        public static void Register(UIRoot root, Canvas canvas) {
            if (canvas == null) {
                return;
            }
            _canvases.AddOrUpdate(root, canvas);
        }

        public static Canvas Get(this UIRoot root) {
            return _canvases.TryGetValue(root, out var canvas) ? canvas : null;
        }
    }
}