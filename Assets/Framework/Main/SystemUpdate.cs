using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface ITurnUpdate {
        void TurnUpdate(bool fullTurn);
    }

    public interface ISystemUpdate {
        bool Unscaled { get; }
        void OnSystemUpdate(float dt);
    }

    public interface IMainSystemUpdate {
        void OnSystemUpdate(float dt);
    }

    public interface IPeriodicUpdate {
        void OnPeriodicUpdate();
    }

    public interface IMainFixedUpdate {
        void OnFixedSystemUpdate();
    }

    public interface ISystemFixedUpdate {
        void OnFixedSystemUpdate(float dt);
    }
}