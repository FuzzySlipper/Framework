using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public interface IAbilityProvider {
        void SetupEntity(Entity entity, AbilityConfig config);
    }

    public class AbilityProviderAttribute : Attribute {
        public readonly string Ability;

        public AbilityProviderAttribute(string ability) {
            Ability = ability;
        }
    }
    public static class AbilityFactory {

        private static Dictionary<string, AbilityConfig> _abilities = new Dictionary<string, AbilityConfig>();
        private static Dictionary<string, AbilityConfig> _abilitiesFullID = new Dictionary<string, AbilityConfig>();
        private static Dictionary<string, IAbilityProvider> _abilityProviders = new Dictionary<string, IAbilityProvider>();

        public static void AddProvider(string ability, IAbilityProvider provider) {
            _abilityProviders.Add(ability, provider);
        }
        
        private static void Init() {
            GameData.AddInit(Init);
            foreach (var loadedDataEntry in GameData.GetSheet("Abilities")) {
                var ability = new AbilityConfig(loadedDataEntry.Value);
                _abilities.AddOrUpdate(loadedDataEntry.Value.ID, ability);
                _abilitiesFullID.AddOrUpdate(loadedDataEntry.Value.FullID, ability);
            }
        }

        public static Entity Get(string id) {
            return BuildEntity(id);
        }

        public static AbilityConfig GetConfig(string id) {
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

        private static Entity BuildEntity(string id) {
            if (_abilities.Count == 0) {
                Init();
            }
            var data = GetConfig(id);
            if (data == null) {
                Debug.LogFormat("{0} didn't load Ability", id);
                return null;
            }
            return BuildAbility(data);
        }

        public static Entity BuildAbility(AbilityConfig abilityConfig) {
            var data = abilityConfig.Data;
            var entity = Entity.New(abilityConfig.Name);
            entity.Add(new TypeId(abilityConfig.ID));
            entity.Add(new LabelComponent(entity.Name));
            entity.Add(new DescriptionComponent(abilityConfig.Description));
            var stats = entity.Add(new StatsContainer());
            var genericData = entity.Add(new GenericDataComponent());
            for (int i = 0; i < abilityConfig.Tags.Count; i++) {
                genericData.SetData(abilityConfig.Tags[i].ID, abilityConfig.Tags[i].Value);
            }
            var icon = abilityConfig.Icon;
            if (!string.IsNullOrEmpty(icon)) {
                entity.Add(new IconComponent(UnityDirs.AbilityIcons, icon));
            }
            
            //entity.Add(new InventoryItem(1, 0, -1));
            entity.Add(new StatusUpdateComponent());
            var actionConfig = entity.Add(new ActionConfig());
            actionConfig.Costs.Add(new ActionPointCost(abilityConfig.ApUsage));
            actionConfig.AnimationTrigger = data.TryGetValue("Animation", GraphTriggers.UseAbility);
            int range = abilityConfig.RangeLimit;
            switch (abilityConfig.Range) {
                case AbilityRange.Weapon:
                    actionConfig.Requirements.Add(new ActionWeaponRangeRequirement(false));
                    break;
                case AbilityRange.MeleeTouch:
                    actionConfig.Requirements.Add(new ActionTouchRangeRequirement());
                    break;
                case AbilityRange.Self:
                    actionConfig.Requirements.Add(new ActionSelfRangeRequirement());
                    break;
                default:
                    range = Mathf.Max(range, 1);
                    actionConfig.Requirements.Add(new ActionRangeRequirement(range));
                    break;
            }
            for (int i = 0; i < abilityConfig.Requirements.Count; i++) {
                switch (abilityConfig.Requirements[i].Config) {
                    case "RangedWeapon":
                        actionConfig.Requirements.Add(new ActionWeaponRequirement(ActionWeaponRequirement.Types.Ranged));
                        break;
                    case "MeleeWeapon":
                        actionConfig.Requirements.Add(new ActionWeaponRequirement(ActionWeaponRequirement.Types.Melee));
                        break;
                    case "Unarmed":
                        actionConfig.Requirements.Add(new ActionWeaponRequirement(ActionWeaponRequirement.Types.Unarmed));
                        break;
                    case "AnyWeapon":
                        actionConfig.Requirements.Add(new ActionWeaponRequirement(ActionWeaponRequirement.Types.Any));
                        break;
                }
            }
            var radius = Mathf.Max(abilityConfig.Radius, 1);
            switch (abilityConfig.ImpactType) {
                case ImpactTypes.Instant:
                    actionConfig.Phases.Add(new InstantActivate());
                    break;
                default:
                case ImpactTypes.TargetHit:
                    if (radius > 1) {
                        actionConfig.Phases.Add(new CheckAreaHit(abilityConfig.Defense, abilityConfig.Stat, radius, false));
                    }
                    else {
                        actionConfig.Phases.Add(new CheckTargetHit(abilityConfig.Defense, abilityConfig.Stat));
                    }
                    break;
                case ImpactTypes.TargetCloseBurst:
                case ImpactTypes.TargetAreaBurst:
                    actionConfig.Phases.Add(new CheckBurstHit(abilityConfig.Defense, abilityConfig.Stat, radius, false));
                    break;
                case ImpactTypes.TargetCloseBlast:
                    actionConfig.Phases.Add(new CheckAreaHit(abilityConfig.Defense, abilityConfig.Stat, radius, false));
                    break;
                case ImpactTypes.TargetAreaWall:
                    actionConfig.Phases.Add(new CheckWallHit(abilityConfig.Defense, abilityConfig.Stat, radius, 2, false));
                    break;
            }
            if (!string.IsNullOrEmpty(abilityConfig.Class) && abilityConfig.Class != "Base") {
                actionConfig.Requirements.Add(new ActionClassRequirement(abilityConfig.Class, abilityConfig.Level));
            }
            if (abilityConfig.Target != TargetType.Any) {
                actionConfig.Requirements.Add(new ActionTargetTypeRequirement(abilityConfig.Target));
            }
            genericData.SetData(AbilityDataEntries.PowerSource, abilityConfig.PowerSource);
            actionConfig.Type = abilityConfig.Type;
            entity.Add(new EntityLevelComponent(abilityConfig.Level));
            genericData.SetData(AbilityDataEntries.DamageType, abilityConfig.DamageType);
            actionConfig.BonusStat = abilityConfig.Stat;
            actionConfig.Focus = abilityConfig.Focus;
            // var spawn = data.Get<DataReference>(DatabaseFields.ActionSpawn);
            var afx = data.GetValue<string>(DatabaseFields.ActionFx);
            if (!string.IsNullOrEmpty(afx)) {
                var actionFx = ItemPool.LoadAsset<ActionFx>(UnityDirs.ActionFx, afx);
                if (actionFx != null) {
                    if (actionFx.TryGetColor(out var actionColor)) {
                        entity.Add(new HitParticlesComponent(actionColor));
                    }
                    entity.Add(new ActionFxComponent(actionFx));
                }
            }
            var customScripting = data.Get<DataList>("ScriptedEvents");
            if (customScripting != null) {
                for (int i = 0; i < customScripting.Count; i++) {
                    var scriptingData = customScripting[i];
                    var eventType = scriptingData.TryGetValue("Event", "");
                    //var eventType = ParseUtilities.TryParseEnum(scriptingData.TryGetValue("Event", ""), ActionState.None);
                    var scripting = scriptingData.TryGetValue("Script", "");
                    if (!string.IsNullOrEmpty(eventType) && !string.IsNullOrEmpty(scripting)) {
                        var customScript = ScriptingSystem.ParseMessage(scripting.SplitIntoWords());
                        if (customScript != null) {
                            actionConfig.AddEvent(eventType, customScript);
                        }
                    }
                }
            }
            if (abilityConfig.TypeComponents != null) {
                World.Get<DataFactory>().AddComponentList(entity, abilityConfig.Data, abilityConfig.TypeComponents);
            }
            for (int i = 0; i < abilityConfig.Actions.Count; i++) {
                var line = abilityConfig.Actions[i];
                var type = line.Type;
                if (!ActionProviderSystem.Providers.TryGetValue(type, out var provider)) {
                    continue;
                }
                provider.SetupEntity(entity, line, data);
                actionConfig.Actions.Add(new ActionProviderEntry(line, provider));
            }
            if (!string.IsNullOrEmpty(abilityConfig.SpecialConfig) && _abilityProviders.TryGetValue(abilityConfig.SpecialConfig, out var abilityProvider)) {
                abilityProvider.SetupEntity(entity, abilityConfig);
            }
            return entity;
        }
    }

    public class AbilityConfig {
        
        public readonly DataEntry Data;
        public readonly string ID;
        public readonly string Name;
        public readonly string Description;
        public readonly string Icon;
        public readonly string Type;
        public readonly string PowerSource;
        public readonly string Class;
        public readonly int Level;
        public readonly string ApUsage;
        public readonly string DamageType;
        public readonly string Defense;
        public readonly string Stat;
        public readonly string Focus;
        public readonly string ImpactType;
        public readonly int Radius;
        public readonly string Range;
        public readonly int RangeLimit;
        public readonly List<SimpleDataLine> Requirements = new List<SimpleDataLine>();
        public readonly TargetType Target;
        public readonly List<SimpleDataLine> Actions = new List<SimpleDataLine>();
        public readonly string SpecialConfig;
        public readonly string ActionFx;
        public readonly string Animation;
        public readonly string ActionSpawn;
        public readonly ValuePairCollection<string> Tags;
        public DataList TypeComponents { get; }

        public AbilityConfig(DataEntry data) {
            Data = data;
            ID = data.ID;
            var itemType = data.Get<DataReference>(ItemTypes.Ability);
            Name = data.GetValue<string>(DatabaseFields.Name);
            Description = data.TryGetValue(DatabaseFields.Description, Name);
            Icon = "A_" + data.GetValue<string>(DatabaseFields.Icon);
            Type = data.GetValue<string>(nameof(Type));
            PowerSource = data.GetValue<string>(nameof(PowerSource));
            Class = data.GetValue<string>(nameof(Class));
            Level = data.GetValue<int>(nameof(Level));
            ApUsage = data.GetValue<string>(nameof(ApUsage));
            DamageType = data.GetValue<string>(nameof(DamageType));
            Defense = data.GetValue<string>(nameof(Defense));
            Stat = data.GetValue<string>(nameof(Stat));
            ImpactType = data.GetValue<string>(nameof(ImpactType));
            Radius = data.GetValue<int>(nameof(Radius));
            Focus = data.GetValue<string>(nameof(Focus));
            Range = data.GetValue<string>(nameof(Range));
            RangeLimit = data.GetValue<int>(nameof(RangeLimit));
            SimpleDataLine.FillList(Requirements, (DataList) data.Get(nameof(Requirements)));
            SimpleDataLine.FillList(Actions, (DataList) data.Get("Actions"));
            Target = ParseUtilities.TryParseEnum(data.TryGetValue("TargetType", "Enemy"), TargetType.Enemy);
            SpecialConfig = data.GetValue<string>(nameof(SpecialConfig));
            ActionFx = data.GetValue<string>(nameof(ActionFx));
            Animation = data.GetValue<string>(nameof(Animation));
            ActionSpawn = data.GetValue<string>(nameof(ActionSpawn));
            SpecialConfig = data.GetValue<string>(nameof(SpecialConfig));
            TypeComponents = itemType?.Value.Get(DatabaseFields.Components) as DataList;
            var tags = (DataList) data.Get(nameof(Tags));
            Tags = new ValuePairCollection<string>(tags, DatabaseFields.ID, DatabaseFields.Value);
        }
    }


    public class SimpleDataLine {
        public string Type { get; }
        public string Target { get; }
        public int Amount { get; }
        public string Config { get; }

        public SimpleDataLine(DataEntry data) {
            Type = data.GetValue<string>(nameof(Type));
            Target = data.GetValue<string>(nameof(Target));
            Amount = data.GetValue<int>(nameof(Amount));
            Config = data.GetValue<string>(nameof(Config));
        }

        public static void FillList(List<SimpleDataLine> simpleDataList, DataList dataList) {
            for (int i = 0; i < dataList.Count; i++) {
                simpleDataList.Add(new SimpleDataLine(dataList[i]));
            }
        }
    }

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
        private string _animationEvent;
        
        public override bool CanResolve(ActionCommand cmd) {
            return cmd.Owner.AnimationEvent.CurrentAnimationEvent == _animationEvent;
        }

        public WaitForAnimationEvent(string animationEvent) {
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
                    var cell = CombatArenaMap.Current.Get(pos);
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
                var cell = CombatArenaMap.Current.Get(pos);
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
                    var cell = CombatArenaMap.Current.Get(pos);
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

    public class ActionProviderEntry {
        public SimpleDataLine Line { get; }
        public IActionProvider Provider { get; }

        public ActionProviderEntry(SimpleDataLine line, IActionProvider provider) {
            Line = line;
            Provider = provider;
        }
    }

    public class ImpactTypes : StringEnum<ImpactTypes> {
        public const string Instant = "Instant";
        public const string TargetHit = "TargetHit";
        public const string TargetCloseBurst = "TargetCloseBurst";
        public const string TargetCloseBlast = "TargetCloseBlast";
        public const string TargetAreaBurst = "TargetAreaBurst";
        public const string TargetAreaWall = "TargetAreaWall";
    }

    public class TargetEventTypes : StringEnum<TargetEventTypes> {
        public const string Attack = "Attack";
        public const string Hit = "Hit";
        public const string Miss = "Miss";
        public const string Effect = "Effect";
        public const string Conjuration = "Conjuration";
        public const string Zones = "Zones";
        public const string Sustain = "Sustain";
        public const string Start = "Start";
    }

    public class AbilityRange : StringEnum<AbilityRange> {
        public const string Self = "Self";
        public const string Melee = "Melee";
        public const string MeleeTouch = "MeleeTouch";
        public const string Weapon = "Weapon";
        public const string Ranged = "Ranged";
        public const string RangedSight = "RangedSight";
    }

    public class AbilityFocus : StringEnum<AbilityFocus> {
        public const string None = "None";
        public const string Weapon = "Weapon";
        public const string Holy = "Holy";
        public const string Orb = "Orb";
        public const string Rod = "Rod";
        public const string Staff = "Staff";
        public const string Tome = "Tome";
        public const string Totem = "Totem";
        public const string Wand = "Wand";
    }
}
