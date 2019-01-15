using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public static class CharacterFactory {
        public static Entity GetBasicCharacterEntity(int faction) {
            var entity = Entity.New("Character");
            StatExtensions.SetupVitalStats(entity);
            StatExtensions.SetupBasicCharacterStats(entity);
            StatExtensions.SetupDefenseStats(entity);
            entity.Add(new LabelComponent(""));
            entity.Add(new DeathStatus());
            entity.Add(new ModifiersContainer(null));
            entity.Add(new StatusContainer());
            entity.Add(new StatusUpdateComponent());
            entity.Add(new GridPosition());
            entity.Add(new PositionComponent());
            entity.Add(new RotationComponent());
            entity.Add(new EquipmentSlots(null));
            entity.Add(new CommandsContainer(null));
            entity.Add(new CommandTarget());
            entity.Add(new FactionComponent(faction));
            entity.Add(new PronounComponent(PlayerPronouns.They));
            return entity;
        }
    }
}
