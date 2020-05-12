using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PixelComrades {
    public static class ActionProviderSystem {

        private static Dictionary<string, IActionProvider> _providers = new Dictionary<string, IActionProvider>();
        
        public static Dictionary<string, IActionProvider> Providers { get => _providers; }

        public static void Add(string type, IActionProvider factory) {
            _providers.AddOrUpdate(type, factory);
        }

        public static void Remove(string type) {
            _providers.Remove(type);
        }

    }
}
