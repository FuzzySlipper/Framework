using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace PixelComrades {
    
    public interface IActionConfig {
<<<<<<< HEAD
        int Range { get; }
        FloatRange Power { get; }
=======
        string Name { get; }
        string Description { get; }
        SpriteReference Icon { get; }
        List<ActionPhases> Phases { get; }
        List<ActionHandler> Handlers { get; }
        int Range { get; }
        FloatRange Power { get; }
        string ToHitStat { get; }
        CollisionType Collision { get; }
>>>>>>> FirstPersonAction
        ImpactRadiusTypes Radius { get; }
        ActionFx ActionFx { get; }
        ScriptedEventConfig[] ScriptedEvents { get; }
        float CritMulti { get; }
        TargetType Targeting { get; }
<<<<<<< HEAD
        string AbilityType { get; }
=======
>>>>>>> FirstPersonAction
        StateGraph ActionGraph { get; }
    }

    public static class ActionProvider {
        public static void AddComponent(Entity entity, IActionConfig data, ActionConfig action) {
            var stats = entity.Get<StatsContainer>();
<<<<<<< HEAD
            stats.Add(new BaseStat(entity, Stats.CriticalMulti, data.CritMulti));
            stats.Add(new RangeStat(entity, Stats.Power, Stats.Power, data.Power.Min, data.Power.Max));
            var targeting = data.Targeting;
            if (targeting == TargetType.Self || targeting == TargetType.Friendly) {
                action.AddEvent(AnimationEvents.Default, new EventGenerateCollisionEvent());
            }
=======
            stats.Add(new BaseStat(entity, Stat.CriticalMulti, data.CritMulti));
            stats.Add(new RangeStat(entity, Stat.Power, Stat.Power, data.Power.Min, data.Power.Max));
            action.Phases.AddRange(data.Phases);
            action.Actions.AddRange(data.Handlers);
            for (int i = 0; i < action.Actions.Count; i++) {
                action.Actions[i].SetupEntity(entity);
            }
            action.Requirements.Add(new ActionTargetTypeRequirement(data.Targeting));
            // if (targeting == TargetType.Self || targeting == TargetType.Friendly) {
            //     action.AddEvent(AnimationEvents.Default, new EventGenerateCollisionEvent());
            // }
>>>>>>> FirstPersonAction
            var radius = data.Radius;
            if (radius != ImpactRadiusTypes.Single) {
                entity.Add(new ImpactRadius(radius, true));
            }
<<<<<<< HEAD
            action.Range = (int) data.Range;
=======
>>>>>>> FirstPersonAction
            if (data.ActionFx != null) {
                entity.Add(new ActionFxComponent(data.ActionFx));
            }
            for (int i = 0; i < data.ScriptedEvents.Length; i++) {
                var scriptingData = data.ScriptedEvents[i];
                var eventType = scriptingData.Event;
                var scripting = scriptingData.Script;
                if (!string.IsNullOrEmpty(eventType) && !string.IsNullOrEmpty(scripting)) {
                    var customScript = ScriptingSystem.ParseMessage(scripting.SplitIntoWords());
                    if (customScript != null) {
                        action.AddEvent(eventType, customScript);
                    }
                }
            }
        }
        //
        // public static void AddCheckForCollision(ActionConfig action, IActionConfig data, bool limitEnemy) {
        //     var raycastSize = ((int) data.Collision) * 0.01f;
        //     switch (data.Collision) {
        //         case CollisionType.Melee:
        //         case CollisionType.MeleeBig:
        //             action.AddEvent(
        //                 AnimationEvents.CollisionOrImpact, new CameraShakeEvent(
        //                     new Vector3
        //                         (0, 0, 1), 4, false));
        //             break;
        //
        //     }
        //     //melee or hitscan need to make that clearer
        //     action.AddEvent(AnimationEvents.Default, new EventCheckRaycastCollision(action.Range, raycastSize, limitEnemy));
        // }
    }
}
