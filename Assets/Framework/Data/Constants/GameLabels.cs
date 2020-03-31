using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class GameText {
        public static string DefaultCurrencyLabel { get { return Currencies.GetNameAt(Game.DefaultCurrencyId); } }
    }
}
