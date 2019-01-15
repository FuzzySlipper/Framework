using System.Collections.Generic;
using System;

namespace PixelComrades {
    public interface IReceive<in T> : IReceive {
        void Handle(T arg);
    }

    public interface IReceiveRef<T> : IReceive {
        void Handle(ref T arg);
    }

    public interface IReceiveGlobal<T> : IReceive {
        void HandleGlobal(ManagedArray<T> arg);
    }

    public interface IReceive {}

    public interface ISignalReceiver {
        void Handle(int signal);
    }

    //public interface IReceiveEvents<in T> : IReceiveEvents where T : IEntityMessage {
    //    void ReceivedEvent(T msg);
    //}

    //public interface IEntityMessage {}
}