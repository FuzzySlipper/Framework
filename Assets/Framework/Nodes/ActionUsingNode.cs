﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionUsingNode : BaseNode {

        private CachedComponent<AnimatorData> _animator = new CachedComponent<AnimatorData>();
        private CachedComponent<StatsContainer> _stats = new CachedComponent<StatsContainer>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        public StatsContainer Stats => _stats.Value;
        public Transform Tr { get => _tr.Value; }
        
        public string LastProcessedAnimationEvent;
        public State CurrentState;
        public ActionEvent ActionEvent;

        public enum State {
            Disabled,
            Starting,
            Running
        }

        private bool _overrideEntityTr = false;

        public IAnimator Animator { get { return _animator?.Value.Animator; } }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _animator, _stats, _tr
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AnimatorData),
            };
        }

        public void Start(ActionEvent actionEvent) {
            ActionEvent = actionEvent;
            ActionEvent.Current.Start(this);
            if (actionEvent.Owner.Tr == null && actionEvent.SpawnPivot != null) {
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
