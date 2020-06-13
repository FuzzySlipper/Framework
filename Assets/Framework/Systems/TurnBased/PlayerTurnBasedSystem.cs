﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerTurnBasedSystem : SystemBase<PlayerTurnBasedSystem> {
    
        private LineRenderer _pathLine;
        private LevelCell _currentMovePnt;

        public static TurnBasedCharacterTemplate Current { get; private set; }
        
        public PlayerTurnBasedSystem() {
            _pathLine = LazyDb.Main.PlayerMovePath;
        }

        public void TurnStart(TurnBasedCharacterTemplate character) {
            character.Pathfinder.Value = CombatPathfinder.GetPathfinder(Game.CombatMap.Cells, character);
            character.Pathfinder.MoveSpeed = World.Get<RulesSystem>().Post(new GatherMoveSpeedEvent(character, 0)).Total;
            SetupPathfindingSprites(character);
            Current = character;
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
                    _pathLine.positionCount = 0;
                }
                return;
            }
            _currentMovePnt = hitCell;
            int moveAp = Current.TurnBased.MoveActions + Current.TurnBased.StandardActions;
            Current.Pathfinder.Value.SetCurrentPath(Current.Location, _currentMovePnt, moveAp, Current.Pathfinder.MoveSpeed);
            _pathLine.positionCount = Current.Pathfinder.Value.CurrentPath.Count;
            for (int i = 0; i < Current.Pathfinder.Value.CurrentPath.Count; i++) {
                var cell = Current.Pathfinder.Value.CurrentPath[i];
                _pathLine.SetPosition(i, cell.PositionV3);
            }
        }

        private bool TryMoveTo(TurnBasedCharacterTemplate character, LevelCell pos) {
            if (character.Pathfinder.Value.CurrentPath.Count <= 0) {
                return false;
            }
            var moveCmd = CommandSystem.GetCommand<MoveCommand>(character);
            moveCmd.CurrentPath = character.Pathfinder.Value.CurrentPath;
            moveCmd.MoveCost = character.Pathfinder.Value.CurrentPathCost;
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
        }

        private void SetupPathfindingSprites(TurnBasedCharacterTemplate character) {
            ClearPathfinding();
            character.Pathfinder.Value.FillReachable(character.Location.Cell, character.Pathfinder.MoveSpeed * 2);
            for (int i = 0; i < character.Pathfinder.Value.ReachableNodes.Count; i++) {
                var node = character.Pathfinder.Value.ReachableNodes[i];
                var color = node.StartCost <= character.Pathfinder.MoveSpeed ? LazyDb.Main.MovementAp1Color : LazyDb.Main.MovementAp2Color;
                var pos = LazySceneReferences.main.Pathfinding.WorldToCell(node.Value.PositionV3);
                LazySceneReferences.main.Pathfinding.SetTile(pos, LazyDb.Main.PathfindCell);
                LazySceneReferences.main.Pathfinding.SetColor(pos, color);
            }
        }

        private void ClearPathfinding() {
            LazySceneReferences.main.Pathfinding.ClearAllTiles();
            _pathLine.positionCount = 0;
            _currentMovePnt = null;
        }
    }
}
