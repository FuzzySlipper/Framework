﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    
    public enum ActionState {
        Start = 0,
        Activate = 1,
        Miss = 2,
        Impact = 3,
        Collision = 4,
        CollisionOrImpact = 5,
        FxOn = 6,
        FxOff = 7,
        None
        //Start, Activate, Miss, Impact, Collision, CollisionOrImpact, FxOn, FxOff, None
    }

    public struct ActionEvent : IEntityMessage {
        public BaseActionTemplate Action { get; }
        public CharacterTemplate Origin { get; }
        public CharacterTemplate Target { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public ActionState State { get; }

        public ActionEvent(BaseActionTemplate action, CharacterTemplate origin, CharacterTemplate target, Vector3 position, Quaternion rotation, ActionState state) {
            Action = action;
            Origin = origin;
            Target = target;
            Position = position;
            Rotation = rotation;
            State = state;
        }

        public ActionEvent(BaseActionTemplate action, CharacterTemplate origin, Entity target, ActionState state) {
            Action = action;
            Origin = origin;
            Target = target.GetTemplate<CharacterTemplate>();
            Position = target.GetPosition();
            Rotation = target.GetRotation();
            State = state;
        }

        public ActionEvent(CharacterTemplate origin, CharacterTemplate target, Vector3 position, Quaternion rotation, ActionState state) {
            var action = origin.Entity.GetTemplate<ActionCharacterTemplate>();
            Action = action?.CurrentAction;
            Origin = origin;
            Target = target;
            Position = position;
            Rotation = rotation;
            State = state;
        }

        public ActionEvent(Entity origin, Entity focus, Vector3 position, Quaternion rotation, ActionState state) {
            Origin = origin.FindTemplate<CharacterTemplate>();
            Target = focus.FindTemplate<CharacterTemplate>();
            var action = origin.GetTemplate<ActionCharacterTemplate>();
            Action = action?.CurrentAction;
            Position = position;
            Rotation = rotation;
            State = state;
        }
    }
    
    public static class ActionEventExtensions{
        public static bool GetSpawnPositionRotation(this ActionEvent ae, out Vector3 pos, out Quaternion rot) {
            if (ae.Action != null) {
                var actionEntity = ae.Action.Entity;
                var spawnPivot = actionEntity.Get<SpawnPivotComponent>();
                if (spawnPivot != null) {
                    pos = spawnPivot.position;
                    rot = spawnPivot.rotation;
                    return true;
                }
                var actionTr = actionEntity.Get<TransformComponent>();
                if (actionTr != null) {
                    pos = actionTr.position;
                    rot = actionTr.rotation;
                    return true;
                }
                var actionPivots = ae.Origin.Entity.Get<ActionPivotsComponent>();
                if (actionPivots != null) {
                    pos = (ae.Action.Config.Type == PivotTypes.Primary ? actionPivots.PrimaryPivot : actionPivots.SecondaryPivot).position;
                    rot = (ae.Action.Config.Type == PivotTypes.Primary ? actionPivots.PrimaryPivot : actionPivots.SecondaryPivot).rotation;
                    return true;
                }
            }
            var transformComponent = ae.Origin.Tr;
            if (transformComponent != null) {
                pos = transformComponent.position;
                rot = transformComponent.rotation;
                return true;
            }
            pos = ae.Position;
            rot = ae.Rotation;
            return false;
        }
    }
}
