using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Priority_Queue;

namespace PixelComrades {
    public enum TurnBasedState {
        Inactive,
        NoUnits,
        Performing,
        WaitingOnInput,
        TurnEnded,
        Disabled,
    }

    public static class TurnBased {

        public const float RecoveryNeededToEndTurn = 100;
        public static int TurnNumber { get { return World.Get<TurnBasedSystem>().TurnCounter; } }
        
    }

    public interface ITurnBasedUnit {
        int Owner { get; set; }
        float Speed { get; }
        void TurnUpdate(float turnEnergy);
        bool TryStartTurn();
    }

    public struct StartTurnEvent : IEntityMessage{}
    public struct EndTurnEvent : IEntityMessage{}

    public class TurnBasedSystem : SystemBase, IMainSystemUpdate {

        private class TurnNode : IComparable<TurnNode>, IComparable {

            public ITurnBasedUnit Value;
            public float Priority;
            public void Clear() {
                Value = null;
                Priority = 999;
            }

            public override bool Equals(object obj) {
                return obj is TurnNode node && node.Value == Value;
            }

            public bool Equals(TurnNode node) {
                return node.Value == Value;
            }

            public override int GetHashCode() {
                if (Value != null) {
                    return Value.GetHashCode();
                }
                return Priority.GetHashCode();
            }

            public int CompareTo(object obj) {
                return obj is TurnNode pn ? CompareTo(pn) : -1;
            }

            public int CompareTo(TurnNode other) {
                return Priority.CompareTo(other.Priority);
            }
        }

        private class NodeSorter : Comparer<TurnNode> {
            public override int Compare(TurnNode x, TurnNode y) {
                if (x == null || y == null) {
                    return 0;
                }
                return -1 * x.Priority.CompareTo(y.Priority);
            }
        }
        
        private static List<TurnNode> _active = new List<TurnNode>();
        private static List<ITurnBasedUnit> _queueActivate = new List<ITurnBasedUnit>();
        private static TurnBasedState _turnState = TurnBasedState.Inactive;
        private static GameOptions.CachedFloat _turnRecoveryAmount = new GameOptions.CachedFloat("TurnRecoverAmount");
        private static GenericPool<TurnNode> _nodePool = new GenericPool<TurnNode>(50, t => t.Clear());

        private float _turnLengthCounter = 0;
        private ITurnBasedUnit _current;
        private NodeSorter _nodeSorter = new NodeSorter();

        public static TurnBasedState TurnState {  get { return _turnState; } }
        public int TurnCounter { get; private set; }
        private float TurnRecoverAmount { get { return _turnRecoveryAmount.Value * TimeManager.DeltaTime; } }

        public static void Add(ITurnBasedUnit unit) {
            if (!_queueActivate.Contains(unit) && GetNode(unit) == null) {
                _queueActivate.Add(unit);
            }
        }

        public void TurnStats() {
            Debug.LogFormat("Turn: {0} Active {1}, Queue {2} State {3}", TurnCounter,_active.Count, _queueActivate.Count, _turnState);
        }

        public static void Remove(ITurnBasedUnit unit) {
            if (_queueActivate.Contains(unit)) {
                _queueActivate.Remove(unit);
            }
            else {
                var node = GetNode(unit);
                if (node != null) {
                    _active.Remove(node);
                    _nodePool.Store(node);
                }
            }
        }

        private static TurnNode GetNode(ITurnBasedUnit unit) {
            for (int i = 0; i < _active.Count; i++) {
                if (_active[i].Value == unit) {
                    return _active[i];
                }
            }
            return null;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            PrepareTurn();
            TurnUpdate();
            //if (Game.Debug) {
            //    DebugText.UpdatePermText("Turn Status", string.Format("{0} : {1}", main.TurnCounter, _turnState));
            //}
        }

        public void Clear() {
            _active.Clear();
            _queueActivate.Clear();
        }

        public void NewTurn() {
            _turnLengthCounter = 0;
            SystemManager.TurnUpdate(true);
            TurnCounter++;
            _turnState = TurnBasedState.TurnEnded;
        }

        private void PrepareTurn() {
            for (int i = 0; i < _queueActivate.Count; i++) {
                var node = _nodePool.New();
                node.Value = _queueActivate[i];
                _active.Add(node);
            }
            _queueActivate.Clear();
            for (int i = 0; i < _active.Count; i++) {
                _active[i].Priority = _active[i].Value.Speed;
            }
        }

        private void TurnUpdate() {
            if (TurnCancel()) {
                return;
            }
            //_active.Sort(_nodeSorter);
            //_active.BubbleSort((i, i1) => i.Priority < i1.Priority);
            for (int i = 0; i < _active.Count; i++) {
                if (TurnCancel()) {
                    return;
                }
                if (_active[i] == null || _active[i].Value == null) {
                    continue;
                }
                _active[i].Value.TurnUpdate(TurnRecoverAmount);
                //need to check it is the first activation
                if (_active[i].Value.TryStartTurn()) {
                    EntityController.GetEntity(_active[i].Value.Owner).Post(EntitySignals.TurnReady);
                    if (GameOptions.TurnBased) {
                        _current = _active[i].Value;
                        break;
                    }
                }
            }
            _turnLengthCounter += TurnRecoverAmount;
            if (_turnLengthCounter >= TurnBased.RecoveryNeededToEndTurn) {
                NewTurn();
            }
            else {
                SystemManager.TurnUpdate(false);
                _turnState = TurnBasedState.Performing;
            }
        }

        private bool TurnCancel() {
            if (_active.Count == 0) {
                _turnState = TurnBasedState.NoUnits;
                return true;
            }
            if (_current != null) {
                if (_current.TryStartTurn()) {
                    _turnState = TurnBasedState.Performing;
                    return true;
                }
                _current = null;
            }
            if (!Game.GameActive || Game.Paused) {
                _turnState = Game.GameActive ? TurnBasedState.WaitingOnInput : TurnBasedState.Disabled;
                return true;
            }
            return false;
        }
    }
}