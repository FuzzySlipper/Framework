using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FactionSystem : SystemBase {

        private Dictionary<int, List<int>> _factionToEnemies = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> _factionToAllies = new Dictionary<int, List<int>>();
        
        public void RegisterFriendlyFaction(int faction, int other) {
            if (!_factionToAllies.TryGetValue(faction, out var factionList)) {
                factionList = new List<int>();
                _factionToAllies.Add(faction, factionList);
            }
            factionList.Add(other);
            if (!_factionToAllies.TryGetValue(other, out var otherList)) {
                otherList = new List<int>();
                _factionToAllies.Add(other, otherList);
            }
            otherList.Add(faction);
        }

        public void RegisterEnemyFaction(int faction, int other) {
            if (!_factionToEnemies.TryGetValue(faction, out var factionList)) {
                factionList = new List<int>();
                _factionToEnemies.Add(faction, factionList);
            }
            factionList.Add(other);
            if (!_factionToEnemies.TryGetValue(other, out var otherList)) {
                otherList = new List<int>();
                _factionToEnemies.Add(other, otherList);
            }
            otherList.Add(faction);
        }

        public void FillFactionEnemiesList(List<Entity> entityList, int faction) {
            if (!_factionToEnemies.TryGetValue(faction, out var list)) {
                return;
            }
            var factionList = EntityController.GetComponentArray<FactionComponent>();
            factionList.RunAction(
                f => {
                    if (list.Contains(f.Faction)) {
                        entityList.Add(f.GetEntity());
                    }
                });
        }

        public void FillFactionFriendsList(List<Entity> entityList, int faction) {
            if (!_factionToAllies.TryGetValue(faction, out var list)) {
                return;
            }
            var factionList = EntityController.GetComponentArray<FactionComponent>();
            factionList.RunAction(
                f => {
                    if (list.Contains(f.Faction)) {
                        entityList.Add(f.GetEntity());
                    }
                });
        }

        public bool AreFriends(int faction, int other) {
            if (faction == other) {
                return true;
            }
            return _factionToAllies.TryGetValue(faction, out var list) && list.Contains(other);
        }

        public bool AreEnemies(int faction, int other) {
            return _factionToEnemies.TryGetValue(faction, out var list) && list.Contains(other);
        }

        public bool AreFriends(Entity source, Entity target) {
            if (source == null || target == null) {
                return false;
            }
            return AreFriends(source.Find<FactionComponent>().Faction, target.Find<FactionComponent>().Faction);
        }

        public bool AreEnemies(Entity source, Entity target) {
            if (source == null || target == null) {
                return false;
            }
            return AreEnemies(source.Find<FactionComponent>().Faction, target.Find<FactionComponent>().Faction);
        }
    }
}
