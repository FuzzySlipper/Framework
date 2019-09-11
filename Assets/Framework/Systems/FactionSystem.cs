using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;

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
            foreach (FactionComponent f in factionList) {
                if (list.Contains(f.Faction)) {
                    var entity = f.GetEntity();
                    if (entity != null) {
                        entityList.Add(entity);
                    }
                }
            }
            //factionList.RunAction(
            //    f => {
            //        if (list.Contains(f.Faction)) {
            //            entityList.Add(f.GetEntity());
            //        }
            //    });
        }

        public void FillFactionFriendsList(List<Entity> entityList, int faction) {
            if (!_factionToAllies.TryGetValue(faction, out var list)) {
                return;
            }
            var factionList = EntityController.GetComponentArray<FactionComponent>();
            foreach (FactionComponent f in factionList) {
                if (list.Contains(f.Faction)) {
                    entityList.Add(f.GetEntity());
                }
            }
            //factionList.RunAction(
            //    f => {
            //        if (list.Contains(f.Faction)) {
            //            entityList.Add(f.GetEntity());
            //        }
            //    });
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

    public struct TagTimerEvent  {

        public Entity Entity { get; }
        public float TimeEnd { get; }
        public int Tag { get; }

        public TagTimerEvent(Entity entity, float timeEnd, int tag) {
            Entity = entity;
            TimeEnd = timeEnd;
            Tag = tag;
        }
    }

    public struct ConfusionEvent : IEntityMessage {
        public float Length { get; }
        public Entity Entity { get; }
        public bool Active { get; }

        public ConfusionEvent(Entity entity, float length, bool active ) {
            Length = length;
            Entity = entity;
            Active = active;
        }
    }

    public struct StunEvent : IEntityMessage {
        public float Length { get; }
        public Entity Entity { get; }
        public bool Active { get; }

        public StunEvent(Entity entity, float length, bool active) {
            Length = length;
            Entity = entity;
            Active = active;
        }
    }

    public struct SlowEvent : IEntityMessage {
        public float Length { get; }
        public Entity Entity { get; }
        public bool Active { get; }

        public SlowEvent(Entity entity, float length, bool active) {
            Length = length;
            Entity = entity;
            Active = active;
        }
    }

    public class TagTimerSystem : SystemBase, ISystemUpdate, IReceiveGlobal<ConfusionEvent>, IReceiveGlobal<SlowEvent>, IReceiveGlobal<StunEvent> {

        private ManagedArray<TagTimerEvent> _timerArray = new ManagedArray<TagTimerEvent>();

        public bool Unscaled { get { return false; } }
        
        public void OnSystemUpdate(float dt) {
            if (_timerArray.UsedCount == 0) {
                return;
            }
            foreach (TagTimerEvent timer in _timerArray) {
                if (timer.TimeEnd > TimeManager.Time) {
                    continue;
                }
                timer.Entity.Tags.Remove(timer.Tag);
                switch (timer.Tag) {
                    case EntityTags.IsConfused:
                        if (EntityCount(timer.Entity, timer.Tag) <= 1) {
                            timer.Entity.Post(new ConfusionEvent(timer.Entity, 0, false));
                        }
                        break;
                    case EntityTags.IsSlowed:
                        if (EntityCount(timer.Entity, timer.Tag) <= 1) {
                            timer.Entity.Post(new SlowEvent(timer.Entity, 0, false));
                        }
                        break;
                    case EntityTags.IsStunned:
                        if (EntityCount(timer.Entity, timer.Tag) <= 1) {
                            timer.Entity.Post(new StunEvent(timer.Entity, 0, false));
                        }
                        break;
                }
                _timerArray.Remove(timer);
            }
        }

        private int EntityCount(Entity entity, int tag) {
            int cnt = 0;
            foreach (TagTimerEvent timerEvent in _timerArray) {
                if (timerEvent.Entity == entity && timerEvent.Tag == tag) {
                    cnt++;
                }
            }
            return cnt;
        }

        public void HandleGlobal(ConfusionEvent arg) {
            if (!arg.Active) {
                return;
            }
            arg.Entity.Tags.Add(EntityTags.IsConfused);
            if (arg.Length > 0) {
                _timerArray.Add(new TagTimerEvent(arg.Entity, TimeManager.Time + arg.Length, EntityTags.IsConfused));
            }
        }

        public void HandleGlobal(SlowEvent arg) {
            if (!arg.Active) {
                return;
            }
            arg.Entity.Tags.Add(EntityTags.IsSlowed);
            if (arg.Length > 0) {
                _timerArray.Add(new TagTimerEvent(arg.Entity, TimeManager.Time + arg.Length, EntityTags.IsSlowed));
            }
        }

        public void HandleGlobal(StunEvent arg) {
            if (!arg.Active) {
                return;
            }
            arg.Entity.Tags.Add(EntityTags.IsStunned);
            if (arg.Length > 0) {
                _timerArray.Add(new TagTimerEvent(arg.Entity, TimeManager.Time + arg.Length, EntityTags.IsStunned));
            }
        }
    }
}
