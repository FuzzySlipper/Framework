using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    
    public interface IActionConfig {
        ActionDistance Range { get; }
        FloatRange Power { get; }
        CollisionType Collision { get; }
        ImpactRadiusTypes Radius { get; }
        ActionFx ActionFx { get; }
        ScriptedEventConfig[] ScriptedEvents { get; }
        float CritMulti { get; }
        TargetType Targeting { get; }
        string AbilityType { get; }
        StateGraph ActionGraph { get; }
    }

    public static class ActionProvider {
        public static void AddComponent(Entity entity, IActionConfig data, ActionConfig action) {
            var stats = entity.Get<StatsContainer>();
            stats.Add(new BaseStat(entity, Stats.CriticalMulti, data.CritMulti));
            stats.Add(new RangeStat(entity, Stats.Power, Stats.Power, data.Power.Min, data.Power.Max));
            var targeting = data.Targeting;
            if (targeting == TargetType.Self || targeting == TargetType.Friendly) {
                action.AddEvent(AnimationEvents.Default, new EventGenerateCollisionEvent());
            }
            var radius = data.Radius;
            if (radius != ImpactRadiusTypes.Single) {
                entity.Add(new ImpactRadius(radius, true));
            }
            action.Range = (int) data.Range;
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
