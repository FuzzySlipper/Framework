﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class NpcTurnBasedSystem : SystemBase {
        
        List<CombatPathfinder.LevelCellNode> _moves = new List<CombatPathfinder.LevelCellNode>();
        private List<ActionTemplate> _tempActionList = new List<ActionTemplate>();
        public NpcTurnBasedSystem(){}

        public void TurnStart(TurnBasedCharacterTemplate character) {
            character.Pathfinder.Value = CombatPathfinder.GetPathfinder(Game.CombatMap.Cells, character);
            character.Pathfinder.MoveSpeed = World.Get<RulesSystem>().Post(new GatherMoveSpeedEvent(character, 0)).Total;
            character.Pathfinder.Value.FillReachable(character.Location.Cell, character.Pathfinder.MoveSpeed*2);
            PathfindingDisplaySystem.Get.SetupPathfindingSprites(character);

            var targetList = character.Faction == (int) Factions.PlayerAllies ? CombatArenaSystem.Enemies : CombatArenaSystem.Friendlies;
            SortByDistanceAsc(targetList, character.Location.Cell.Position);
            var attackRange = character.GetDefaultAttackRange()*2;
            if (targetList[0].Position.Value.DistanceCheb(character.Position) <= attackRange) {
                SetupActionOnTarget(character, targetList[0]);
                return;
            }
            for (int i = 0; i < targetList.Count; i++) {
                var target = targetList[i];
                if (target.IsDead) {
                    continue;
                }
                _moves.Clear();
                for (int d = 0; d < DirectionsExtensions.DiagonalLength; d++) {
                    var cell = Game.CombatMap.Get(target.Position + ((DirectionsEight) d).ToPoint3());
                    if (cell == null) {
                        continue;
                    }
                    var node = character.Pathfinder.Value.GetNode(cell);
                    _moves.Add(node);
                }
                if (_moves.Count == 0) {
                    continue;
                }
                SortByDistanceAsc(_moves);
                for (int m = 0; m < _moves.Count; m++) {
                    character.Pathfinder.Value.SetCurrentPath(character.Location, _moves[i].Value, 2, character.Pathfinder.MoveSpeed);
                    if (character.Pathfinder.Value.CurrentPath.Count <= 1) {
                        continue;
                    }
                    var moveCmd = CommandSystem.GetCommand<MoveCommand>(character);
                    moveCmd.CurrentPath = character.Pathfinder.Value.CurrentPath;
                    if (moveCmd.TryStart(false)) {
                        character.Target.Target = target;
                        PathfindingDisplaySystem.Get.SetCurrentPath(character);
                        return;
                    }
                    CommandSystem.Store(moveCmd);
                }
            }
            CommandSystem.GetCommand<IdleCommand>(character).TryStart(false);
        }

        private void SetupActionOnTarget(TurnBasedCharacterTemplate origin, CharacterTemplate target) {
            _tempActionList.Clear();
            for (int i = 0; i < origin.ActionSlots.Count; i++) {
                var action = origin.ActionSlots.GetSlot(i).Action;
                if (action != null) {
                    _tempActionList.Add(action);
                }
            }
            _tempActionList.Shuffle();
            for (int i = 0; i < _tempActionList.Count; i++) {
                var action = _tempActionList[i];
                if (!action.CanAct(origin.Entity, target)) {
                    continue;
                }
                if (!action.Config.CanTarget(action, origin, target)) {
                    continue;
                }
                var actionCommand = CommandSystem.GetCommand<ActionCommand>(origin);
                actionCommand.Action = action;
                if (actionCommand.TryStart(false)) {
                    PathfindingDisplaySystem.Get.ClearDisplay();
                    return;
                }
                CommandSystem.Store(actionCommand);
            }
            CommandSystem.GetCommand<IdleCommand>(origin).TryStart(false);
        }

        public void TurnContinue(TurnBasedCharacterTemplate character) {
            if (character.TurnBased.StandardActions > 0) {
                SetupActionOnTarget(character, character.Target.TargetChar);
            }
            else {
                character.TurnBased.Clear();
                TurnBasedSystem.Get.CommandComplete(character);
            }
        }

        public void TurnEnd(TurnBasedCharacterTemplate character) {
            CombatPathfinder.Store(character.Pathfinder.Value);
            character.Pathfinder.Value = null;
            PathfindingDisplaySystem.Get.ClearDisplay();
        }

        private void SortByDistanceAsc(List<CombatPathfinder.LevelCellNode> list) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].StartCost > list[sort + 1].StartCost) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }

        private void SortByDistanceAsc(List<TurnBasedCharacterTemplate> list, Point3 center) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].Location.Cell.Position.DistanceCheb(center) > list[sort + 1].Location.Cell.Position.DistanceCheb(center)) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }
    }
}
