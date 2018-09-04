using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace PixelComrades {
    
    public enum PlayerPronouns {
        [Description("he")] He,
        [Description("she")] She,
        [Description("they")] They
    }

    public enum MenuPivot {
        Center = 0,
        Right = 1,
        Left = 2,
        Icon = 3,
    }

    public enum Directions {
        Forward = 0,
        Right = 1,
        Back = 2,
        Left = 3,
        Up = 4,
        Down = 5,
        None = 99,
    }

    public enum DirectionsEight {
        Front = 0,
        FrontRight = 1,
        Right = 2,
        RearRight = 3,
        Rear = 4,
        RearLeft = 5,
        Left = 6,
        FrontLeft = 7,
        Top = 8,
        Bottom = 9,
    }

    public enum ActionDistance {
        Short = 0,
        Extended = 1,
        Medium = 2,
        Far = 3,
        Infinite = 4,
    }

}
