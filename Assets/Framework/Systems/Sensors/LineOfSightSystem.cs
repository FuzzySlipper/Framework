using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class LineOfSightSystem : SystemBase {

        public LayerMask UnitCheckMask = LayerMasks.DefaultCollision;
        public LayerMask CollisionCheck = LayerMasks.DefaultCollision;
        public LayerMask EnvironmentMask = LayerMasks.Environment;

        private RaycastHit[] _rayHits = new RaycastHit[35];

        public bool CanSee(Entity source, Entity target) {
            var sourcePos = source.GetPosition();
            var dir = target.GetPosition() - sourcePos;
            var ray = new Ray(sourcePos, dir.normalized);
            var rayLimit = Physics.RaycastNonAlloc(ray, _rayHits, dir.magnitude, UnitCheckMask);
            return CanSeeTarget(rayLimit, source, target);
        }

        public bool CanSee(Entity source, Vector3 target) {
            var sourcePos = source.GetPosition();
            var dir = target - sourcePos;
            var ray = new Ray(sourcePos, dir.normalized);
            var rayLimit = Physics.RaycastNonAlloc(ray, _rayHits, dir.magnitude, CollisionCheck);
            return CanSeeTarget(rayLimit, source, null);
        }

        public bool CanSeeOrHear(Entity source, Entity target, out bool canHear) {
            var sourcePos = source.GetPosition();
            var dir = target.GetPosition() - sourcePos;
            var ray = new Ray(sourcePos, dir.normalized);
            var rayLimit = Physics.RaycastNonAlloc(ray, _rayHits, dir.magnitude, UnitCheckMask);
            _rayHits.SortByDistanceAsc(rayLimit);
            canHear = true;
            for (int i = 0; i < rayLimit; i++) {
                if (_rayHits[i].transform.CompareTag(StringConst.TagEnvironment)) {
                    canHear = false;
                    continue;
                }
                var entity = UnityToEntityBridge.GetEntity(_rayHits[i].collider);
                if (entity == null) {
                    continue;
                }
                if (entity == source) {
                    continue;
                }
                if (entity == target) {
                    return true;
                }
            }
            return true;
        }

        public Entity GetTarget(Entity source, Vector3 dir, float range, TargetType targeting) {
            //if (GameOptions.ArenaCombat) {
            //    return FindArenaActorInRange(source, range, targeting);
            //}
            var sourcePos = source.GetPosition();
            var ray = new Ray(sourcePos, dir);
            var rayLimit = Physics.RaycastNonAlloc(ray, _rayHits, range, UnitCheckMask);
            _rayHits.SortByDistanceAsc(rayLimit);
            for (int i = 0; i < rayLimit; i++) {
                if (_rayHits[i].transform.CompareTag(StringConst.TagEnvironment)) {
                    return null;
                }
                var entity = UnityToEntityBridge.GetEntity(_rayHits[i].collider);
                if (entity == null) {
                    continue;
                }
                if (entity == source) {
                    continue;
                }
                var target = EntityController.GetEntity(entity);
                if (targeting == TargetType.Enemy && !World.Get<FactionSystem>().AreEnemies(source, target)) {
                    continue;
                }
                if (targeting == TargetType.Friendly && !World.Get<FactionSystem>().AreFriends(source, target)) {
                    continue;
                }
                return target;
            }
            return null;
        }


        //private Entity FindArenaActorInRange(Entity source, float range, TargetType targeting) {
        //    List<Entity> actorList = new List<Entity>();
        //    switch (targeting) {
        //        case TargetType.Self:
        //            return source;
        //        case TargetType.Friendly:
        //            World.Get<FactionSystem>().FillFactionFriendsList(actorList, source.Get<FactionComponent>().Faction);
        //            break;
        //        default:
        //            World.Get<FactionSystem>().FillFactionEnemiesList(actorList, source.Get<FactionComponent>().Faction);
        //            break;
        //    }
        //    actorList.Shuffle();
        //    var ranksCanBust = (int) range;
        //    for (int i = 0; i < actorList.Count; i++) {
        //        if (actorList[i].PositionRank <= ranksCanBust) {
        //            return actorList[i];
        //        }
        //    }
        //    return null;
        //}

        private bool CanSeeTarget(int rayLimit, Entity source, Entity target) {
            _rayHits.SortByDistanceAsc(rayLimit);
            for (int i = 0; i < rayLimit; i++) {
                if (_rayHits[i].transform.CompareTag(StringConst.TagEnvironment)) {
                    return false;
                }
                var colliderEntity = UnityToEntityBridge.GetEntity(_rayHits[i].collider);
                if (colliderEntity == null || colliderEntity == source) {
                    continue;
                }
                if (colliderEntity == target) {
                    return true;
                }
                if (World.Get<FactionSystem>().AreEnemies(colliderEntity, source)) {
                    return false;
                }
            }
            return true;
        }

        public bool IsTargetVisible(Entity owner, Vector3 source, Entity unit, Vector3 forward, float maxVision) {
            var targetTr = unit.Tr;
            var targetDir = (targetTr.position - source);
            var targetDistance = targetDir.sqrMagnitude;
            var maxSquared = maxVision * maxVision;
            if (targetDistance > maxSquared) {
                return false;
            }
            targetDir = targetDir.normalized;
            var targetAngle = Vector3.Angle(forward, targetDir);
            if (targetDistance <= Game.MapCellSize) {
                return true; // Player is too close to the npc and will be detected, regardless of whether it sees it or not
            }
            if (targetAngle > 90 / 2.0f) {
                return false; // Not within the field of vision
            }
            var offset = Vector3.up;
            var direction = (targetTr.position - source).normalized;
            var hitLimit = Physics.RaycastNonAlloc(source + offset, direction, _rayHits, maxSquared * 0.5f);
            for (int write = 0; write < hitLimit; write++) {
                for (int sort = 0; sort < hitLimit - 1; sort++) {
                    if (_rayHits[sort].distance > _rayHits[sort + 1].distance) {
                        var temp = _rayHits[sort + 1];
                        _rayHits[sort + 1] = _rayHits[sort];
                        _rayHits[sort] = temp;
                    }
                }
            }
            for (int i = 0; i < hitLimit; i++) {
                if (_rayHits[i].collider.isTrigger || _rayHits[i].collider.CompareTag(StringConst.TagEnemy)) {
                    continue;
                }
                if (_rayHits[i].collider.CompareTag(StringConst.TagPlayer)) {
                    return true;
                }
                return false;
            }
            return true;
        }

    }
}
