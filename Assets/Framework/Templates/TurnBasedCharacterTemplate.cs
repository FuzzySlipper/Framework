using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PixelComrades {
    public class TurnBasedCharacterTemplate : CharacterTemplate {
        private CachedComponent<BlockCellLocation> _location = new CachedComponent<BlockCellLocation>();
        private CachedComponent<TurnBasedComponent> _turnBased = new CachedComponent<TurnBasedComponent>();
        private CachedComponent<CombatPathfinderComponent> _pathfinder = new CachedComponent<CombatPathfinderComponent>();


        public CombatPathfinderComponent Pathfinder { get => _pathfinder; }
        public TurnBasedComponent TurnBased { get => _turnBased; }
        public BlockCellLocation Location { get => _location; }

        public override List<CachedComponent> GatherComponents {
            get {
                var list = base.GatherComponents;
                list.Add(_location);
                list.Add(_turnBased);
                list.Add(_pathfinder);
                return list;
            }
        }


        public override System.Type[] GetTypes() {
            var list = base.GetTypes().ToList();
            list.Add(typeof(CombatPathfinderComponent));
            return list.ToArray();
        }

        public float GetDefaultAttackRange() {
            return 2;
        }
    }
}