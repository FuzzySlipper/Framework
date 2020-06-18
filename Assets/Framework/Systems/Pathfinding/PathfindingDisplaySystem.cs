using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PathfindingDisplaySystem : SystemBase<PathfindingDisplaySystem> {

        private Tilemap _pathfindTileMap;

        public PathfindingDisplaySystem() {
            _pathfindTileMap = LazySceneReferences.main.Pathfinding;
        }

        public void SetupPathfindingSprites(TurnBasedCharacterTemplate character) {
            ClearDisplay();
            for (int i = 0; i < character.Pathfinder.Value.ReachableNodes.Count; i++) {
                var node = character.Pathfinder.Value.ReachableNodes[i];
                var color = node.StartCost <= character.Pathfinder.MoveSpeed ? LazyDb.Main.MovementAp1Color : LazyDb.Main.MovementAp2Color;
                var pos = _pathfindTileMap.WorldToCell(node.Value.PositionV3);
                _pathfindTileMap.SetTile(pos, LazyDb.Main.PathfindCell);
                _pathfindTileMap.SetTileFlags(pos, TileFlags.None);
                _pathfindTileMap.SetColor(pos, color);
            }
            for (int i = 0; i < character.Pathfinder.Value.CurrentPath.Count; i++) {
                var node = character.Pathfinder.Value.CurrentPath[i];
                // _pathLine.SetPosition(i, cell.PositionV3);
                var pos = _pathfindTileMap.WorldToCell(node.PositionV3);
                _pathfindTileMap.SetTile(pos, LazyDb.Main.PathfindCell);
                _pathfindTileMap.SetTileFlags(pos, TileFlags.None);
                _pathfindTileMap.SetColor(pos, LazyDb.Main.MovementPathColor);
            }
        }

        public void SetCurrentPath(TurnBasedCharacterTemplate character) {
            // _pathLine.positionCount = Current.Pathfinder.Value.CurrentPath.Count;
            SetupPathfindingSprites(character);
        }

        public void ClearDisplay() {
            _pathfindTileMap.ClearAllTiles();
            // _pathLine.positionCount = 0;
        }
    }
}
