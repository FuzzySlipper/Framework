using System.Collections.Generic;
using System;

namespace PixelComrades {
    public interface IReceive {}
    
    public interface IReceive<in T> : IReceive {
        void Handle(T arg);
    }

    public interface IReceiveGlobalArray<T> : IReceive {
        void HandleGlobal(BufferedList<T> arg);
    }

    public interface IReceiveGlobal<in T> : IReceive {
        void HandleGlobal(T arg);
    }

    //public interface IReceiveEvents<in T> : IReceiveEvents where T : IEntityMessage {
    //    void ReceivedEvent(T msg);
    //}

    //public interface IEntityMessage {}
}