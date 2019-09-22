using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionUsingNode : BaseNode {

        private CachedComponent<AnimatorComponent> _animator = new CachedComponent<AnimatorComponent>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        public StatsContainer Stats => _stats.Value;
        public TransformComponent Tr { get => _tr.Value; }
        
        public string LastProcessedAnimationEvent;
        public State CurrentState;
        public ActionEvent ActionEvent;

        public enum State {
            Disabled,
            Starting,
            Running
        }

        public IAnimator Animator { get { return _animator?.Value.Value; } }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _animator, _stats, _tr
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AnimatorComponent),
            };
        }

        public void Start(ActionEvent actionEvent) {
            ActionEvent = actionEvent;
            ActionEvent.Current.Start(this);
        }

        public void Stop() {
            CurrentState = State.Disabled;
        }

        public void AdvanceEvent() {
            World.Get<ActionSystem>().AdvanceEvent(this);
        }

        public void ParentSpawn(Transform spawn) {
            if (ActionEvent.SpawnPivot != null) {
                spawn.SetParentResetPos(ActionEvent.SpawnPivot);
            }
            else {
                Tr.SetChild(spawn);
                spawn.ResetPos();
            }
        }
    }

    public interface IActionEvent {
        void Trigger(ActionUsingNode node, string eventName);
    }
}
