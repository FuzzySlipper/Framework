using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IPlayerFlightController {
        void NewGame();
        void SetStatus(bool status);
        Transform Tr { get;}
        Entity Entity { get;}
        void SetMode(FlightControl.Mode mode);
        float Thrust { get;}
    }
}
