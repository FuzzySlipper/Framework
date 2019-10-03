using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XNode;

namespace PixelComrades {
    public abstract class AnimationNode : BaseAnimationNode {
        
        [Input] public BaseAnimationNode EnterNode;
        [Output] public BaseAnimationNode ExitNode;
        public float Duration;
        public SimpleAnimationEvent[] Events = new SimpleAnimationEvent[0];
        
    }

    public abstract class RuntimeAnimationNode : RuntimeStateNode {
        
        private float _duration;
        private SimpleAnimationEvent[] _events;
        private List<SimpleAnimationEvent> _pendingEvents = new List<SimpleAnimationEvent>();
        private RuntimeStateNode _exitNode;
        
        public override RuntimeStateNode GetExitNode() {
            return _exitNode;
        }

        public RuntimeAnimationNode(AnimationNode node, RuntimeStateGraph owner) : base(owner) {
            _duration = node.Duration;
            _events = node.Events;
            _exitNode = owner.CreateRuntimeNode(node.ExitNode);
            
        }
        

        public override void OnEnter(RuntimeStateNode lastNode) {
            base.OnEnter(lastNode);
            _pendingEvents.Clear();
            _pendingEvents.AddRange(_events);
        }

        public override bool TryComplete() {
            var time = TimeManager.Time - TimeEntered;
            var percent = time / _duration;
            for (int i = _pendingEvents.Count - 1; i >= 0; i--) {
                var pendingEvent = _pendingEvents[i];
                if (pendingEvent.Time <= percent) {
                    Owner.CompletedEvents.Add(pendingEvent.Event);
                    _pendingEvents.RemoveAt(i);
                }
            }
            UpdateAnimation(percent);
            return percent >= 1;
        }

        protected abstract void UpdateAnimation(float percent);
    }

    public abstract class RuntimeStateNode {
        public RuntimeStateNode LastEnter { get; protected set; }
        public RuntimeStateGraph Owner { get; }
        public float TimeEntered { get; protected set; }

        protected RuntimeStateNode(RuntimeStateGraph owner) {
            Owner = owner;
        }
        
        public virtual void OnEnter(RuntimeStateNode previous) {
            LastEnter = previous;
            TimeEntered = TimeManager.Time;
        }
        public virtual void OnExit() {}
        
        public abstract RuntimeStateNode GetExitNode();

        public abstract bool TryComplete();
        
    }

    public abstract class RuntimeStateGraph {
        
        public List<string> CompletedEvents = new List<string>();
        
        private Dictionary<string, RuntimeStateNode> _lookup = new Dictionary<string, RuntimeStateNode>();
        public float TimeStartGraph { get; protected set; }
        public RuntimeStateNode Current { get; protected set; }
        public RuntimeStateNode StartNode { get; protected set; }

        public bool IsActive { get { return Current != null; } }
        public virtual void Start() {
            TimeStartGraph = TimeManager.Time;
            CompletedEvents.Clear();
            SetCurrentNode(StartNode);
        }

        public virtual void Update() {
            if (Current.TryComplete()) {
                SetCurrentNode(Current.GetExitNode());
            }
        }

        protected virtual void SetCurrentNode(RuntimeStateNode node) {
            var last = Current;
            if (last != null) {
                last.OnExit();
            }
            Current = node;
            if (Current == null) {
                OnComplete();
                return;
            }
            Current.OnEnter(last);
        }
        public virtual void OnComplete(){}

        public RuntimeStateNode GetRuntimeNode(string id) {
            return _lookup.TryGetValue(id, out var node) ? node : null;
        }
        
        public RuntimeStateNode CreateRuntimeNode(ConvertibleNode node) {
            if (_lookup.TryGetValue(node.Id, out var existing)) {
                return existing;
            }
            var runtimeNode = node.GetRuntimeNode(this);
            _lookup.Add(node.Id, runtimeNode);
            //_allNodes.Add(runtimeNode);
            return runtimeNode;
        }
        
    }

    public abstract class BaseAnimationNode : ConvertibleNode {
        
        
        
    }

    public abstract class ConvertibleNode : Node {
        public string Id { get; }

        protected ConvertibleNode() {
            Id = new System.Guid().ToString();
        }
        
        public abstract RuntimeStateNode GetRuntimeNode(RuntimeStateGraph owner);
        
        public override object GetValue(NodePort port) {
            return "";
        }
    }

    [System.Serializable]
    public struct SimpleAnimationEvent {
        public string Event;
        public float Time;
    }
}
