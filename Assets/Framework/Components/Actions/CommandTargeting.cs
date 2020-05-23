using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public struct CommandTargeting {
        public TargetType Criteria;
        public float Range;
        public bool RequireLoS;

        public CommandTargeting(TargetType criteria, float range, bool requireLoS) {
            Criteria = criteria;
            Range = range;
            RequireLoS = requireLoS;
        }

        //public Vector3 GetMaxRangePosition() {
        //    this.GetEntity().Spawn(out var startPos, out var rotation);
        //    return startPos + ((rotation * Vector3.forward) * Range);
        //}

        //public Entity GetTarget() {
        //    var owner = this.GetEntity();
        //}

        //private bool RayBlocked(Entity target) {
        //    if (GameOptions.ArenaCombat) {
        //        return CheckRayBlockedArena(target);
        //    }
        //    var dir = (target.WorldCenter - CenterPosition);
        //    _ray.origin = CenterPosition;
        //    _ray.direction = dir;
        //    var limit = Physics.RaycastNonAlloc(_ray, _raycastHits, LayerMasks.DefaultCollision);
        //    _raycastHits.SortByDistanceAsc(limit);
        //    for (int l = 0; l < limit; l++) {
        //        var hit = _raycastHits[l];
        //        if (hit.collider == Owner.Collider) {
        //            continue;
        //        }
        //        var actor = Actor.Get(hit.collider);
        //        if (actor == target) {
        //            return false;
        //        }
        //        if (actor != null && actor.Faction == Owner.Faction) {
        //            continue;
        //        }
        //        if (actor == null && LayerMasks.Environment.ContainsLayer(hit.transform.gameObject.layer)) {
        //            LastStatusUpdate = "Environment Blocked!";
        //            return true;
        //        }
        //        if (actor != null && !actor.IsDead) {
        //            LastStatusUpdate = "Enemy Blocked!";
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private bool CheckRayBlockedArena(Entity target) {
        //    var dir = (target.WorldCenter - Owner.WorldCenter).normalized;
        //    var endPoint = target.GridPosition;
        //    var pos = Owner.WorldCenter;
        //    int cnter = 5000;
        //    var ranksCanBust = (int) Range;
        //    while (cnter > 0) {
        //        pos = pos + (dir * CombatArenaController.CellSize);
        //        var p3 = CombatArenaController.Current.WorldToGrid(pos);
        //        var cell = CombatArenaController.Current.GetCell(p3);
        //        if (cell == null) {
        //            cnter--;
        //            if (p3 == endPoint) {
        //                break;
        //            }
        //            continue;
        //        }
        //        if (cell.Actor != null) {
        //            if (cell.Actor == target) {
        //                return false;
        //            }
        //            if (!cell.Actor.IsDead && cell.Actor.Faction.IsEnemy(Owner.Faction)) {
        //                if (ranksCanBust < cell.Actor.PositionRank + Owner.PositionRank) {
        //                    LastStatusUpdate = "Enemy Blocked!";
        //                    return true;
        //                }
        //            }
        //        }
        //        cnter--;
        //    }
        //    return false;
        //}
    }

    public static class CommandTargetingExtensions {
        public static bool SatisfiesCondition(this CommandTargeting targeting, Entity owner, CommandTarget cmdTarget, bool postUpdates = true) {
            if (targeting.Criteria == TargetType.Any && targeting.Range <= 0 || owner == null) {
                return true;
            }
            var target = cmdTarget.Target;
            if (target != null && !targeting.SatisfiesCondition(owner, target, postUpdates)) {
                return false;
            }
            if (targeting.RequireLoS) {
                if (target == null && !World.Get<LineOfSightSystem>().CanSee(owner, cmdTarget.GetPosition)) {
                    if (postUpdates) {
                        owner.PostAll(new StatusUpdate(owner,"Can't see target", Color.yellow));
                    }
                    return false;
                }
            }
            if (target == null && targeting.Range > 0.25f) {
                var distance = DistanceSystem.GetDistance(owner, cmdTarget.GetPosition);
                if (distance > targeting.Range) {
                    if (postUpdates) {
                        owner.PostAll(new StatusUpdate(owner, distance + " distance out of range", Color.yellow));
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool SatisfiesCondition(this CommandTargeting targeting, Entity owner, Entity target, bool postUpdates = true) {
            if (targeting.Criteria == TargetType.Any && targeting.Range <= 0) {
                return true;
            }
            if (owner == null || target == null) {
                return true;
            }
            switch (targeting.Criteria) {
                case TargetType.Enemy:
                    if (!World.Get<FactionSystem>().AreEnemies(owner, target)) {
                        if (postUpdates) {
                            owner.PostAll(new StatusUpdate(owner, "Not an enemy", Color.yellow));
                        }
                        return false;
                    }
                    break;
                case TargetType.Friendly:
                    if (!World.Get<FactionSystem>().AreFriends(owner, target)) {
                        if (postUpdates) {
                            owner.PostAll(new StatusUpdate(owner, "Not friendly", Color.yellow));
                        }
                        return false;
                    }
                    break;
                case TargetType.Self:
                    if (target.Id != owner) {
                        if (postUpdates) {
                            owner.PostAll(new StatusUpdate(owner, "Self only", Color.yellow));
                        }
                        return false;
                    }
                    break;
            }
            if (targeting.RequireLoS) {
                if (!World.Get<LineOfSightSystem>().CanSee(owner, target)) {
                    if (postUpdates) {
                        owner.PostAll(new StatusUpdate(owner, "Can't see target", Color.yellow));
                    }
                    return false;
                }
            }
            if (targeting.Range > 0.25f) {
                var dist = DistanceSystem.GetUnitDistance2D(owner, target);
                return  dist <= targeting.Range;
            }
            return true;
        }


    }

    public enum TargetType {
        Any=0,
        Enemy=1,
        Friendly=2,
        Self=3,
    }

    public enum AdvancedTargeting {
        Self,
        Enemy,
        EnemyStrong,
        EnemyWeak,
        EnemyHurt,
        Friendly,
        FriendlyStrong,
        FriendlyWeak,
        FriendlyHurt,
    }
}
