﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerTurnBasedSystem : SystemBase<PlayerTurnBasedSystem> {
    
        private LevelCell _currentMovePnt;

        public static TurnBasedCharacterTemplate Current { get; private set; }
        
        public PlayerTurnBasedSystem() {
        }

        public void TurnStart(TurnBasedCharacterTemplate character) {
            character.Pathfinder.Value = CombatPathfinder.GetPathfinder(Game.CombatMap.Cells, character);
            character.Pathfinder.MoveSpeed = World.Get<RulesSystem>().Post(new GatherMoveSpeedEvent(character, 0)).Total;
            character.Pathfinder.Value.FillReachable(character.Location.Cell, character.Pathfinder.MoveSpeed * 2);
            Current = character;
            PathfindingDisplaySystem.Get.SetupPathfindingSprites(character);
        }

        public void TurnContinue(TurnBasedCharacterTemplate character) {
            if (character.TurnBased.ActionPoints == 0 && character.TurnBased.MoveActions == 0) {
                CommandSystem.GetCommand<IdleCommand>(character).TryStart(false);
            }
        }

        public void OnMoveClick(Vector3 hitPnt) {
            var hitCell = Game.CombatMap.Get(hitPnt);
            if (hitCell == _currentMovePnt) {
                if (TryMoveTo(Current, hitCell)) {
                    ClearMove();
                }
                return;
            }
            _currentMovePnt = hitCell;
            int moveAp = Current.TurnBased.MoveActions + Current.TurnBased.StandardActions;
            Current.Pathfinder.Value.SetCurrentPath(Current.Location, _currentMovePnt, moveAp, Current.Pathfinder.MoveSpeed);
            PathfindingDisplaySystem.Get.SetCurrentPath(Current);
        }

        private bool TryMoveTo(TurnBasedCharacterTemplate character, LevelCell pos) {
            if (character.Pathfinder.Value.CurrentPath.Count <= 0) {
                return false;
            }
            var moveCmd = CommandSystem.GetCommand<MoveCommand>(character);
            moveCmd.CurrentPath = character.Pathfinder.Value.CurrentPath;
            if (moveCmd.TryStart(false)) {
                return true;
            }
            CommandSystem.Store(moveCmd);
            return false;
        }

        public void TurnEnd(TurnBasedCharacterTemplate character) {
            CombatPathfinder.Store(character.Pathfinder.Value);
            character.Pathfinder.Value = null;
            Current = null;
            ClearMove();
        }

        private void ClearMove() {
            _currentMovePnt = null;
            PathfindingDisplaySystem.Get.ClearDisplay();
        }
            
    }
}
