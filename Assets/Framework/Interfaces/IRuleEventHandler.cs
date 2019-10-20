using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IRuleEventHandler {}

    public interface IRuleEventStart<T> : IRuleEventHandler where T : IRuleEvent {
        bool CanRuleEventStart(ref T context);
    }

    public interface IRuleEventRun<T> : IRuleEventHandler where T : IRuleEvent {
        void RuleEventRun(ref T context);
    }

    public interface IRuleEventEnded<T> : IRuleEventHandler where T : IRuleEvent {
        void RuleEventEnded(ref T context);
    }
}
