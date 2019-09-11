using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionUsingNode : BaseNode {

        private CachedComponent<AnimatorData> _animator = new CachedComponent<AnimatorData>();

        public string LastProcessedAnimationEvent;
        public State CurrentState;
        public ActionEvent ActionEvent;

        public enum State {
            Disabled,
            Starting,
            Running
        }

        private bool _overrideEntityTr = false;

        public IAnimator Animator { get { return _animator?.c.Animator; } }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {_animator};

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AnimatorData),
            };
        }

        public void Start(ActionEvent actionEvent) {
            ActionEvent = actionEvent;
            ActionEvent.Current.Start(this);
            if (actionEvent.ActionEntity.Tr == null && actionEvent.SpawnPivot != null) {
                _overrideEntityTr = true;
                actionEvent.ActionEntity.Tr = actionEvent.SpawnPivot;
            }
        }

        public void Stop() {
            CurrentState = State.Disabled;
            if (_overrideEntityTr) {
                _overrideEntityTr = false;
                ActionEvent.ActionEntity.Tr = null;
            }
        }

        public void AdvanceEvent() {
            World.Get<ActionSystem>().AdvanceEvent(this);
        }
    }

    public interface IActionEvent {
        void Trigger(ActionUsingNode node, string eventName);
    }
}
