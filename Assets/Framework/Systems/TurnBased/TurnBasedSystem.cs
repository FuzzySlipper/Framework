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

    public struct StartTurnEvent : IEntityMessage {
        public Entity Entity { get; }

        public StartTurnEvent(Entity entity) {
            Entity = entity;
        }
    }

    public struct EndTurnEvent : IEntityMessage {
        public Entity Entity { get; }

        public EndTurnEvent(Entity entity) {
            Entity = entity;
        }
    }

    public class TurnBasedSystem : SystemBase<TurnBasedSystem> {

        private static TurnBasedState _turnState = TurnBasedState.Inactive;

        private TurnBasedCharacterTemplate _current;
        
        public static TurnBasedState TurnState {  get { return _turnState; } }
        public static int TurnNumber { get; private set; }

        private TemplateList<TurnBasedCharacterTemplate> _turnTemplates;
        private ManagedArray<TurnBasedCharacterTemplate>.RefDelegate _findCurrentFastest;
        private ManagedArray<TurnBasedCharacterTemplate>.RefDelegate _setupUnitTurns;

        public TurnBasedSystem() {
            TemplateFilter<TurnBasedCharacterTemplate>.Setup();
            _turnTemplates = EntityController.GetTemplateList<TurnBasedCharacterTemplate>();
            _findCurrentFastest = FindCurrent;
            _setupUnitTurns = SetupUnitTurn;
        }

        public void TurnStats() {
            Debug.LogFormat("Turn: {0} State {1}", TurnNumber, _turnState);
        }

        private void FindCurrent(ref TurnBasedCharacterTemplate template) {
            if (template.TurnBased.TurnNumber != TurnNumber) {
                SetupUnitTurn(ref template);    
            }
            if (template.TurnBased.ActionPoints <= 0) {
                return;
            }
            if (_current == null || template.TurnBased.Speed > _current.TurnBased.Speed) {
                _current = template;
            }
        }

        private void SetupUnitTurn(ref TurnBasedCharacterTemplate template) {
            if (template.TurnBased.InitiativeRoll < 0) {
                template.TurnBased.InitiativeRoll = RulesSystem.CalculateD20Roll(1);
                template.TurnBased.InitiativeStatBonus = RulesSystem.CalculateStatsWithLog(template.Stats.Get(Stats.Agility), (int) template.Stats.GetValue(Stats.Level));
                var resultEvent = World.Get<RulesSystem>().Post(new RollInitiativeEvent(template, template.TurnBased.InitiativeRoll, template.TurnBased.InitiativeStatBonus));
                template.TurnBased.InitiativeStatBonus = resultEvent.Bonus;
                var logSystem = World.Get<GameLogSystem>();
                logSystem.StartNewMessage(out var logMsg, out var hoverMsg);
                logMsg.Append(template.GetName());
                logMsg.Append(" initiative: ");
                logMsg.Append(resultEvent.Total);
                logMsg.Append(" ");
                hoverMsg.AppendNewLine(RulesSystem.LastQueryString.ToString());
                logSystem.PostCurrentStrings(GameLogSystem.NormalColor);
            }
            else {
                template.TurnBased.InitiativeStatBonus = RulesSystem.CalculateStatsWithLog(template.Stats.Get(Stats.Agility), (int) template.Stats.GetValue(Stats.Level));
                var resultEvent = World.Get<RulesSystem>().Post(new RollInitiativeEvent(template, template.TurnBased.InitiativeRoll, template.TurnBased.InitiativeStatBonus));
                template.TurnBased.InitiativeStatBonus = resultEvent.Bonus;
            }
            template.TurnBased.StandardActions = 1;
            template.TurnBased.MinorActions = 1;
            template.TurnBased.MoveActions = 1;
            template.TurnBased.TurnNumber = TurnNumber;
        }

        public void CommandComplete(TurnBasedCharacterTemplate template) {
            if (template == null || template.TurnBased.ActionPoints == 0) {
                if (_current != null) {
                    _current.Entity.Post(new EndTurnEvent(_current.Entity));
                    if (_current.IsPlayer()) {
                        World.Get<PlayerTurnBasedSystem>().TurnEnd(_current);
                    }
                    else {
                        World.Get<NpcTurnBasedSystem>().TurnEnd(_current);
                    }
                }
                _turnTemplates.Run(_findCurrentFastest);
                if (_current == null) {
                    NewTurn();
                }
            }
            else {
                if (_current != template) {
                    _current = template;
                    StartTurn();
                }
                else {
                    RunTurn();
                }
            }
        }

        private void StartTurn() {
            _current.Entity.Post(new StartTurnEvent(_current.Entity));
            if (_current.IsPlayer()) {
                World.Get<PlayerTurnBasedSystem>().TurnStart(_current);
            }
            else {
                World.Get<NpcTurnBasedSystem>().TurnStart(_current);
            }
        }

        public void StartTurns() {
            TurnNumber = 0;
            SetupNewTurn();
        }
        
        private void NewTurn() {
            _turnState = TurnBasedState.TurnEnded;
            SystemManager.TurnUpdate(true);
            TurnNumber++;
            SetupNewTurn();
        }

        private void SetupNewTurn() {
            _turnTemplates.Run(_setupUnitTurns);
            _turnTemplates.Run(_findCurrentFastest);
            if (_current == null) {
                _turnState = TurnBasedState.NoUnits;
            }
            else {
                _turnState = TurnBasedState.Performing;
                StartTurn();
            }
        }

        private void RunTurn() {
            if (_current.IsPlayer()) {
                World.Get<PlayerTurnBasedSystem>().TurnContinue(_current);
            }
            else {
                World.Get<NpcTurnBasedSystem>().TurnContinue(_current);
            }
        }
    }
}