using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class CharacterFactory {
        public static Entity GetBasicCharacterEntity(int faction) {
            var entity = Entity.New("Character");
            var stats = entity.Add(new StatsContainer());
            StatExtensions.SetupVitalStats(stats);
            StatExtensions.SetupBasicCharacterStats(stats);
            StatExtensions.SetupDefenseStats(stats);
            entity.Add(new LabelComponent(""));
            entity.Add(new StatusContainer());
            entity.Add(new StatusUpdateComponent());
            entity.Add(new DamageComponent());
            entity.Add(new GridPosition(Point3.zero));
            entity.Add(new CommandTarget());
            entity.Add(new FactionComponent(faction));
            entity.Add(new PronounComponent(PlayerPronouns.They));
            entity.Add(new CurrentAction());
            entity.Add(new AnimationEventComponent());
            entity.Add(new GenericDataComponent());
            entity.Add(new EntityLevelComponent(1));
            return entity;
        }
    }
}
