using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PixelComrades {
    public static class AbilityFactory {

        private static Dictionary<string, AbilityConfig> _abilities = new Dictionary<string, AbilityConfig>();
        private static Dictionary<string, AbilityConfig> _abilitiesFullID = new Dictionary<string, AbilityConfig>();
        
        private static void Init() {
            GameData.AddInit(Init);
            foreach (var loadedDataEntry in GameData.GetSheet("Abilities")) {
                var ability = new AbilityConfig(loadedDataEntry.Value);
                _abilities.AddOrUpdate(loadedDataEntry.Value.ID, ability);
                _abilitiesFullID.AddOrUpdate(loadedDataEntry.Value.FullID, ability);
            }
        }

        public static Entity Get(string id, bool ignoreCost = false) {
            return BuildEntity(id, ignoreCost);
        }

        public static AbilityConfig GetTemplate(string id) {
            if (_abilities.Count == 0) {
                Init();
            }
            var dict = _abilities;
            if (dict.TryGetValue(id, out var data)) {
                return data;
            }
            dict = _abilitiesFullID;
            return dict.TryGetValue(id, out data) ? data : null;
        }

        public static Dictionary<string, AbilityConfig> GetDict() {
            if (_abilities.Count == 0) {
                Init();
            }
            return _abilities;
        }

        private static Entity BuildEntity(string id, bool ignoreVitalCost = false) {
            if (_abilities.Count == 0) {
                Init();
            }
            var data = GetTemplate(id);
            if (data == null) {
                Debug.LogFormat("{0} {1} didn't load Ability", id);
                return null;
            }
            return BuildAbility(data, ignoreVitalCost);
        }

        public static Entity BuildAbility(AbilityConfig data, bool ignoreVitalCost) {
            var entity = Entity.New(data.Name);
            entity.Add(new TypeId(data.ID));
            entity.Add(new LabelComponent(entity.Name));
            entity.Add(new DescriptionComponent(data.Description));
            entity.Add(new StatsContainer());
            var icon = data.Icon;
            if (!string.IsNullOrEmpty(icon)) {
                entity.Add(new IconComponent(UnityDirs.AbilityIcons, icon));
            }
            //entity.Add(new InventoryItem(1, 0, -1));
            entity.Add(new StatusUpdateComponent());
            //if (data.ActionSpawn != null) {
            //    entity.Add(new ActionSpawnComponent(data.ActionSpawn));
            //}
            //entity.Add(new CommandTargeting(data.Target, GameData.ActionDistance.GetAssociatedValue(data.Range), true));
            //var cmd = new ActionSequence(data.CommandsElements.ToArray());
            //cmd.Costs.Add(new RecoveryCost(data.Recovery));
            //if (!ignoreVitalCost) {
            //    cmd.Costs.Add(new CostVital(data.TargetVital, data.Cost));
            //}
            //World.Get<DataFactory>().AddComponent(entity, data.Data, typeof(Action));
            entity.Add(new DataDescriptionComponent(data.DataDescription));
            if (data.TypeComponents != null) {
                World.Get<DataFactory>().AddComponentList(entity, data.Data, data.TypeComponents);
            }
            return entity;
        }
    }

    public class AbilityConfig {

        public ActionCommandType CommandType;
        public string Name;
        public string Description;
        public string Skill;
        public string Icon;
        public string ID;
        public int Level;
        public ActionSource Source;
        public TargetType Target;
        public DataEntry Data;
        public string DataDescription;
        public DataList TypeComponents { get; }

        public AbilityConfig(DataEntry data) {
            ID = data.ID;
            var itemType = data.Get<DataReference>(DatabaseFields.ItemType);
            Name = data.GetValue<string>(DatabaseFields.Name);
            Description = data.TryGetValue(DatabaseFields.Description, Name);
            Icon = "A_" + data.GetValue<string>(DatabaseFields.Icon);
            Source = ParseUtilities.TryParseEnum(data.TryGetValue("ActionSource", ""), ActionSource.Melee);
            Target = ParseUtilities.TryParseEnum(data.TryGetValue("TargetType", "Enemy"), TargetType.Enemy);
            var range = data.TryGetValue("Range", "Medium");
            var cost = data.TryGetValue("Cost", 1f);
            Level = data.TryGetValue("Level", 1);
            CommandType = ParseUtilities.TryParseEnum(data.TryGetValue(DatabaseFields.Radius, "Command"), ActionCommandType.Attack);
            Skill = data.TryGetValue(DatabaseFields.Skill, "");
            TypeComponents = itemType?.Value.Get(DatabaseFields.Components) as DataList;
            Data = data;
            //CommandsElements.Add(new DetermineHitOrMiss(ActionStateEvents.None));
            //if (!string.IsNullOrEmpty(Animation)) {
            //    CommandsElements.Add(new WaitForAnimation(ActionStateEvents.None, Animation, _defaultAnimationTimeout, true, false));
            //}
            //if (data.Get<DataReference>(DatabaseFields.Projectile) != null) {
            //    CommandsElements.Add(new WaitForSpawnMovement(ActionStateEvents.None));
            //}
            //CommandsElements.Add(new GenerateCollisionEvent(ActionStateEvents.Activate));
            var radius = ParseUtilities.TryParseEnum(data.TryGetValue(DatabaseFields.Radius, "Single"), ImpactRadiusTypes.Single);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<b>Power:</b> ");
            sb.Append(data.TryGetValue(DatabaseFields.PowerMin, 0f));
            sb.Append(data.TryGetValue(DatabaseFields.PowerMax, 1f));
            sb.Append("<b>Cost:</b> ");
            sb.Append(cost.ToString("F0"));
            sb.Append(" ");
            sb.Append(GameData.Vitals.GetNameAt("Vitals.Energy"));
            sb.Append(System.Environment.NewLine);
            sb.Append("<b>Targeting:</b> ");
            sb.NewLineAppend(Target.ToDescription());
            sb.Append("<b>Radius:</b> ");
            sb.NewLineAppend(radius.ToDescription());
            sb.Append("<b>Range:</b> ");
            sb.NewLineAppend(range);
            sb.Append("<b>Recovery:</b> ");
            sb.NewLineAppend(CommandType.ToDescription());
            DataDescription = sb.ToString();
        }
    }
}
