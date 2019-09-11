using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IPlayerFlightController {
        void SetStatus(bool status);
        void NewGame();
        Transform Tr { get;}
        Rigidbody Rb { get;}
        Entity Entity { get;}
        void SetDefaultMode();
        void SetMode(FlightControl.Mode mode);
        float Thrust { get;}
    }
}
