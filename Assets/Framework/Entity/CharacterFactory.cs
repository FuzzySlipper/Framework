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
            entity.Add(new GridPosition());
            entity.Add(new CommandTarget());
            entity.Add(new FactionComponent(faction));
            entity.Add(new PronounComponent(PlayerPronouns.They));
            entity.Add(new CurrentAction());
            entity.Add(new AnimationEventComponent());
            entity.Add(new GenericDataComponent());
            entity.Add(new ModifierListComponent());
            entity.Add(new EntityLevelComponent(1));
            entity.Add(new AbilitiesContainer());
            var equip = entity.Add(new EquipmentSlots());
            EquipmentSlotExtensions.GatherDefaultSlots(equip);
            AddCombatRating(entity);

            return entity;
        }

        public static Entity GetTurnBasedEntity(int faction) {
            var entity = GetBasicCharacterEntity(faction);
            
            entity.Add(new CombatPathfinderComponent());
            entity.Add(new CellLocation());
            entity.Add(new TurnBasedComponent());

            return entity;
        }

        public static void AddCombatRating(Entity entity) {
            var combatPower = new BaseStat(entity, Stats.CombatRating, "Combat Rating", 0);
            var stats = entity.Get<StatsContainer>();
            stats.Add(combatPower);
            for (int i = 0; i < Attributes.Count; i++) {
                stats.Get(Attributes.GetID(i)).AddDerivedStat(1, combatPower);
            }
        }

        public static Entity GetSpriteTurnBasedEntity(int faction, GameObject spritePrefab, float offset) {
            var entity = GetTurnBasedEntity(faction);
            SetupSpriteEntity(entity, spritePrefab, offset);
            // if (GameOptions.Get("SpriteConvertActors", false)) {
            //     entity.Add(new ConvertedSprite(entity.Get<RenderingComponent>()));
            // }
            return entity;
        }

        public static void SetupSpriteEntity(Entity entity, GameObject spritePrefab, float offset) {
            var prefab = ItemPool.SpawnScenePrefab(spritePrefab, Vector3.zero, Quaternion.identity);
            var spriteHolder = prefab.GetComponent<SpriteHolder>();
            if (spriteHolder == null) {
                ItemPool.Despawn(prefab);
                return;
            }
            spriteHolder.SpriteTr.localPosition = new Vector3(0, offset, 0);
            UnityToEntityBridge.RegisterToEntity(prefab.gameObject, entity);
            entity.Add(new TransformComponent(prefab.transform));
            entity.Add(new SpriteBillboardComponent(spriteHolder.Facing, spriteHolder.Backwards, spriteHolder.Billboard));
            spriteHolder.SetComponent(entity.Add(new RenderingVisibilityComponent()));
            SpriteRendererComponent renderer = entity.Add(new SpriteRendererComponent(spriteHolder));
            var collider = entity.Add(new SpriteColliderComponent(spriteHolder.SpriteTr.GetComponent<SpriteCollider>()));
            entity.Add(new RenderingComponent(renderer));
            entity.Add(new RigidbodyComponent(prefab.GetComponent<Rigidbody>()));
            entity.Add(new ColliderComponent(collider.Value.UnityCollider));
            entity.Add(new FloatingTextCombatComponent(prefab.transform, new Vector3(0, 1.5f, 0)));
            entity.Add(new SpriteAnimatorComponent());
            entity.Add(new SpriteColorComponent());
        }
    }
}
