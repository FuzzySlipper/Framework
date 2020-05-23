using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class GameConstants : SimpleScriptableDatabase<GameConstants> {

        [SerializeField] private int _unitGrid = 3;
        
        public static int UnitGrid { get => Main._unitGrid; }
    }
}
