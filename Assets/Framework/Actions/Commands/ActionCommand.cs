using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ActionCommand : Command {

        public ActionTemplate Action;
        public HitData HitResult;
        public ActionEvent LastEvent;
        
        private int _phaseIndex;
        
        public override Sprite Icon { get { return null; } }
        
        public override void Clear() {
            base.Clear();
            Action = null;
        }

        public override bool CanStart() {
            if (Action != null && !Action.CanAct(Owner.Entity, null)) {
                return false;
            }
            // if (Owner.Target.TargetChar == null) {
            //     
            // }
            return base.CanStart();
        }

        public override bool TryStart(CharacterTemplate target, bool postUpdates = true) {
            if (!World.Get<CommandSystem>().TryAddCommand(this)) {
                return false;
            }
            _phaseIndex = 0;
            var origin = Owner.Tr.position;
            RunEvent(TargetEventTypes.Start, target, origin, target.Tr.GetLookAtRotation(origin));
            return true;
        }

        public void RunEvent(string eventName, CharacterTemplate target, Vector3 hitPos, Quaternion rot) {
            ActionState state = ActionState.None;
            switch (eventName) {
                case TargetEventTypes.Start:
                    state = ActionState.Start;
                    break;
                case TargetEventTypes.Effect:
                    state = ActionState.Activate;
                    break;
                case TargetEventTypes.Hit:
                    state = ActionState.Impact;
                    break;
                case TargetEventTypes.Miss:
                    state = ActionState.Miss;
                    break;
            }
            if (state != ActionState.None) {
                LastEvent = new ActionEvent(Action, Owner, target, hitPos, rot, state);
                Owner.Post(LastEvent);
            }
            for (int i = 0; i < Action.Config.Actions.Count; i++) {
                var action = Action.Config.Actions[i];
                if (action.TargetEvent == eventName) {
                    action.OnUsage(LastEvent, this);
                }
            }
        }

        public void ProcessHit(HitData hitData, Quaternion rot) {
            HitResult = hitData;
            if (HitResult.Result != CollisionResult.Miss) {
                RunEvent(TargetEventTypes.Hit, hitData.Target, hitData.Point, rot);
                RunEvent(TargetEventTypes.Effect, hitData.Target, hitData.Point, rot);
            }
            else {
                RunEvent(TargetEventTypes.Miss, hitData.Target, hitData.Point, rot);
            }
        }

        public override bool TryComplete() {
            while (true) {
                if (!Action.Config.Phases.HasIndex(_phaseIndex)) {
                    return true;
                }
                if (!Action.Config.Phases[_phaseIndex].CanResolve(this)) {
                    return false;
                }
                _phaseIndex++;
            }
        }

        public void CheckHit(string targetDefense, string bonusAttackStat, CharacterTemplate target) {
            CollisionExtensions.GenerateHitLocDir(Owner.Tr, target.Tr, target.Collider, out var hitPoint, out var dir);
            var hitRot = Quaternion.LookRotation(dir);
            RunEvent(TargetEventTypes.Effect, target, hitPoint, hitRot);
            RunEvent(TargetEventTypes.Attack, target, hitPoint, hitRot);
            var attackRoll = new CheckHitEvent(Action, Owner, target, targetDefense);
            var abilityStat = Action.Stats.Get(Stats.Attack);
            if (abilityStat != null) {
                attackRoll.AttackTotal += abilityStat.Value;
            }
            var bonusStat = Owner.Stats.Get(bonusAttackStat);
            if (bonusStat != null) {
                attackRoll.AttackTotal += bonusStat.D20ModifierValue;
            }
            var hit = new HitData(World.Get<RulesSystem>().Post(attackRoll).Result, target, hitPoint, dir);
            ProcessHit(hit, hitRot);
        }
    }
}
