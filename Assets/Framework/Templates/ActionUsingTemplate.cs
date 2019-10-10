using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionUsingTemplate : BaseTemplate {

        private CachedComponent<AnimatorComponent> _animator = new CachedComponent<AnimatorComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<CurrentAction> _currentAction = new CachedComponent<CurrentAction>();
        public StatsContainer Stats => _stats.Value;
        public TransformComponent Tr { get => _tr.Value; }
        public Action Current => _currentAction.Value.Value;
        
        public IAnimator Animator { get { return _animator?.Value.Value; } }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _animator, _stats, _tr, _currentAction
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AnimatorComponent),
                typeof(CurrentAction)
            };
        }
    }
}
