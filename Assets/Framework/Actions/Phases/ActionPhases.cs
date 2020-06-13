using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
     public abstract class ActionPhases {
        public abstract bool CanResolve(ActionCommand cmd);
    }

    public class StartAnimation : ActionPhases {
        private string _animation;

        public override bool CanResolve(ActionCommand cmd) {
            cmd.Owner.AnimGraph.TriggerGlobal(_animation);
            return true;
        }

        public StartAnimation(string animation) {
            _animation = animation;
        }
    }

    public class WaitForAnimationEvent : ActionPhases {
        private AnimationEvent.Type _animationEvent;
        
        public override bool CanResolve(ActionCommand cmd) {
            return cmd.Owner.AnimationEvent.CurrentAnimationEvent.EventType == _animationEvent;
        }

        public WaitForAnimationEvent(AnimationEvent.Type animationEvent) {
            _animationEvent = animationEvent;
        }
    }

    public class CheckTargetHit : ActionPhases {
        private string _targetDefense;
        private string _bonusStat;

        public CheckTargetHit(string targetDefense, string bonusStat) {
            _targetDefense = targetDefense;
            _bonusStat = bonusStat;
        }

        public override bool CanResolve(ActionCommand cmd) {
            var target = cmd.Owner.Target.TargetChar != null ? cmd.Owner.Target.TargetChar : cmd.Owner;
            cmd.CheckHit(_targetDefense, _bonusStat, target);
            return true;
        }
    }

    public class CheckAreaHit : ActionPhases {
        
        private string _targetDefense;
        private string _bonusStat;
        private int _radius;
        private bool _checkRequirements;

        public CheckAreaHit(string targetDefense, string bonusStat, int radius, bool checkRequirements) {
            _targetDefense = targetDefense;
            _bonusStat = bonusStat;
            _radius = radius;
            _checkRequirements = checkRequirements;
        }

        public override bool CanResolve(ActionCommand cmd) {
            var center = cmd.Owner.Target.GetPositionP3;
            for (int x = 0; x < _radius; x++) {
                for (int z = 0; z < _radius; z++) {
                    var pos = center + new Point3(x, 0, z);
                    var cell = Game.CombatMap.Get(pos);
                    if (cell.Unit == null) {
                        continue;
                    }
                    if (_checkRequirements&& !cmd.Action.Config.CanEffect(cmd.Action, cmd.Owner, cell.Unit)) {
                        continue;
                    }
                    cmd.CheckHit(_targetDefense, _bonusStat, cell.Unit);
                }
            }
            return true;
        }
    }

    public class CheckWallHit : ActionPhases {

        private string _targetDefense;
        private string _bonusStat;
        private int _radius;
        private int _axisDirection;
        private bool _checkRequirements;

        public CheckWallHit(string targetDefense, string bonusStat, int radius, int axisDirection, bool checkRequirements) {
            _targetDefense = targetDefense;
            _bonusStat = bonusStat;
            _radius = radius;
            _axisDirection = axisDirection;
            _checkRequirements = checkRequirements;
        }

        public override bool CanResolve(ActionCommand cmd) {
            var center = cmd.Owner.Target.GetPositionP3;
            for (int i = 0; i < _radius; i++) {
                var pos = center;
                pos[_axisDirection] += i;
                var cell = Game.CombatMap.Get(pos);
                if (cell.Unit == null) {
                    continue;
                }
                if (_checkRequirements && !cmd.Action.Config.CanEffect(cmd.Action, cmd.Owner, cell.Unit)) {
                    continue;
                }
                cmd.CheckHit(_targetDefense, _bonusStat, cell.Unit);
            }
            return true;
        }
    }

    public class CheckBurstHit : ActionPhases {

        private string _targetDefense;
        private string _bonusStat;
        private int _radius;
        private bool _checkRequirements;

        public CheckBurstHit(string targetDefense, string bonusStat, int radius, bool checkRequirements) {
            _targetDefense = targetDefense;
            _bonusStat = bonusStat;
            _radius = radius;
            _checkRequirements = checkRequirements;
        }

        public override bool CanResolve(ActionCommand cmd) {
            var center = cmd.Owner.Position;
            for (int x = 0; x < _radius; x++) {
                for (int z = 0; z < _radius; z++) {
                    var pos = center + new Point3(x, 0, z);
                    var cell = Game.CombatMap.Get(pos);
                    if (cell.Unit == null || cell.Unit == cmd.Owner) {
                        continue;
                    }
                    if (_checkRequirements && !cmd.Action.Config.CanEffect(cmd.Action, cmd.Owner, cell.Unit)) {
                        continue;
                    }
                    cmd.CheckHit(_targetDefense, _bonusStat, cell.Unit);
                }
            }
            return true;
        }
    }

    public class InstantActivate : ActionPhases {

        public InstantActivate() { }

        public override bool CanResolve(ActionCommand cmd) {
            var target = cmd.Owner.Target.TargetChar != null ? cmd.Owner.Target.TargetChar : cmd.Owner;
            CollisionExtensions.GenerateHitLocDir(cmd.Owner.Tr, target.Tr, target.Collider, out var hitPoint, out var dir);
            var hitRot = Quaternion.LookRotation(dir);
            var hit = new HitData(CollisionResult.Hit, target, hitPoint, dir);
            cmd.ProcessHit(hit, hitRot);
            return true;
        }
    }
}
