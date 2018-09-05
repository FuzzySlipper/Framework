using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

        public const float RecoveryStepPerSecond = 30;
        public const float RecoveryNeededToEndTurn = 90;

        public const float DefaultRecovery = 135;
        public const float MinBaseRecovery = 68;

        public static StaticNoticeMsg MsgTurn = new StaticNoticeMsg("{0} {1}");
        
        public static int TurnNumber { get { return World.Get<TurnBasedSystem>().TurnCounter; } }

        public static class Events {
            public const int GlobalTurnStarted = 0;
            public const int GlobalTurnEnded = 1;
        }
    }

    public interface ITurnBasedUnit {
        int Owner { get; set; }
        float Speed { get; }
        void TurnUpdate(float turnEnergy);
        bool TurnReady();
    }

    public struct StartTurnEvent : IEntityMessage{}
    public struct EndTurnEvent : IEntityMessage{}

    public class TurnBasedSystem : SystemBase, IMainSystemUpdate {

        
        private static List<ITurnBasedUnit> _active = new List<ITurnBasedUnit>();
        private static List<ITurnBasedUnit> _queueActivate = new List<ITurnBasedUnit>();
        private static TurnBasedState _turnState = TurnBasedState.Inactive;

        private float _turnLengthCounter = 0;
        private ITurnBasedUnit _current;

        public static TurnBasedState TurnState {  get { return _turnState; } }
        public int TurnCounter { get; private set; }
        public List<ITurnBasedUnit> Active { get { return _active; } }
        private float TurnRecoverAmount { get { return TurnBased.RecoveryStepPerSecond * TimeManager.DeltaTime; } }

        public static void Add(ITurnBasedUnit unit) {
            if (!_queueActivate.Contains(unit) && !_active.Contains(unit)) {
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
            else if (_active.Contains(unit)) {
                _active.Remove(unit);
            }
        }

        public void OnSystemUpdate(float dt) {
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
                _active.Add(_queueActivate[i]);
            }
            _queueActivate.Clear();
        }

        private void TurnUpdate() {
            if (TurnCancel()) {
                return;
            }
            _active.Sort(CompareActorsBySpeed);
            for (int i = 0; i < _active.Count; i++) {
                if (TurnCancel()) {
                    return;
                }
                if (_active[i] == null) {
                    continue;
                }
                _active[i].TurnUpdate(TurnRecoverAmount);
                if (_active[i].TurnReady()) {
                    EntityController.GetEntity(_active[i].Owner).Post(EntitySignals.TurnReady);
                    if (GameOptions.TurnBased) {
                        _current = _active[i];
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
                if (_current.TurnReady()) {
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

        private static int CompareActorsBySpeed(ITurnBasedUnit a, ITurnBasedUnit b) {
            if (a.Speed < b.Speed) {
                return 1;
            }

            if (a.Speed > b.Speed) {
                return -1;
            }

            return 0;
        }

    }

}