using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    public class AbilityFactory : ScriptableDatabase<AbilityFactory> {

        [SerializeField] private List<AbilityConfig> _allAbilities = new List<AbilityConfig>();
        
        private static Dictionary<string, AbilityConfig> _abilities = new Dictionary<string, AbilityConfig>();
        public override IEnumerable<UnityEngine.Object> AllObjects { get { return _allAbilities; } }
        public override System.Type DbType { get { return typeof(AbilityConfig); } }

        public override void AddObject(Object obj) {
            var item = obj as AbilityConfig;
            if (item == null || _allAbilities.Contains(item)) {
                return;
            }
            _allAbilities.Add(item);
        }
        
        public override T GetObject<T>(string id) {
            return _abilities.TryGetValue(id, out var target) ? target as T : null;
        } 

        public override string GetId<T>(T obj) {
            return obj is AbilityConfig config ? config.ID : "";
        }

        public override void CleanObjectList() {
            for (int i = _allAbilities.Count - 1; i >= 0; i--) {
                if (_allAbilities[i] == null) {
                    _allAbilities.RemoveAt(i);
                }
            }
        }

        private static void Init() {
            GameData.AddInit(Init);
            _abilities.Clear();
            for (int i = 0; i < Main._allAbilities.Count; i++) {
                var ability = Main._allAbilities[i];
                if (ability == null) {
                    continue;
                }
                _abilities.AddOrUpdate(ability.ID, ability);
            }
        }

        public static Entity GetRandom() {
            return BuildAbility(Main._allAbilities.RandomElement());
        }

        public static Entity Get(string id, bool ignoreCost = false) {
            return BuildEntity(id, ignoreCost);
        }

        public static AbilityConfig GetConfig(string id) {
            if (_abilities.Count == 0) {
                Init();
            }
            return _abilities.TryGetValue(id, out var data) ? data : null;
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
            var data = GetConfig(id);
            if (data == null) {
                Debug.LogFormat("{0} didn't load Ability", id);
                return null;
            }
            return BuildAbility(data);
        }

        public static Entity BuildAbility(AbilityConfig config) {
            var entity = Entity.New(config.Name);
            entity.Add(new TypeId(config.ID));
            entity.Add(new LabelComponent(entity.Name));
            entity.Add(new DescriptionComponent(config.Description));
            entity.Add(new StatsContainer());
            if (config.Icon.IsLoaded) {
                entity.Add(new IconComponent(config.Icon.LoadedAsset, ""));
            }
            else {
                config.Icon.LoadAsset(
                    handle => {
                        entity.Add(new IconComponent(handle, ""));
                    });
            }
            entity.Add(new InventoryItem(1, 0,  ItemRarity.Special));
            entity.Add(new StatusUpdateComponent());
            config.AddComponents(entity);
            // entity.Add(new DataDescriptionComponent(config.DataDescription));
            return entity;
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
}
