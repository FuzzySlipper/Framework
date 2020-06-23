﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerTurnBasedSystem : SystemBase<PlayerTurnBasedSystem> {
    

        public static TurnBasedCharacterTemplate Current { get; private set; }
        
        public PlayerTurnBasedSystem() {
        }

        public void TurnStart(TurnBasedCharacterTemplate character) {
            character.Pathfinder.Value = CombatPathfinder.GetPathfinder(character);
            character.Pathfinder.MoveSpeed = World.Get<RulesSystem>().Post(new GatherMoveSpeedEvent(character, 0)).Total;
            Current = character;
            PlayerControllerSystem.Get.GetController<OverheadStrategyController>().TurnStart();
        }
        
        public void TurnContinue(TurnBasedCharacterTemplate character) {
            if (character.TurnBased.ActionPoints == 0 && character.TurnBased.MoveActions == 0) {
                character.TurnBased.Clear();
                TurnBasedSystem.Get.CommandComplete(character);
            }
            else {
                PlayerControllerSystem.Get.GetController<OverheadStrategyController>().TurnContinue();
            }
        }

        public bool TryMoveToCurrent() {
            if (Current.Pathfinder.Value.CurrentPath.Count <= 0) {
                return false;
            }
            var moveCmd = CommandSystem.GetCommand<MoveCommand>(Current);
            moveCmd.CurrentPath = Current.Pathfinder.Value.CurrentPath;
            if (moveCmd.TryStart(false)) {
                return true;
            }
            CommandSystem.Store(moveCmd);
            return false;
        }

        public void TurnEnd(TurnBasedCharacterTemplate character) {
            CombatPathfinder.Store(character.Pathfinder.Value);
            character.Pathfinder.Value = null;
            PlayerControllerSystem.Get.GetController<OverheadStrategyController>().TurnEnded();
            Current = null;
        }
    }
}
