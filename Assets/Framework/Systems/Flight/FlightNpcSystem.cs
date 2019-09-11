using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightNpcSystem : SystemBase, IMainSystemUpdate {

        private List<NpcFlyingNode> _flyingList;

        public FlightNpcSystem() {
            NodeFilter<NpcFlyingNode>.New(NpcFlyingNode.GetTypes());
        }

        public override void Dispose() {
            base.Dispose();
            if (_flyingList != null) {
                _flyingList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (_flyingList == null) {
                _flyingList = EntityController.GetNodeList<NpcFlyingNode>();
            }
            if (_flyingList == null || Game.Paused || !Game.GameActive) {
                return;
            }
            for (int i = 0; i < _flyingList.Count; i++) {
                var npc = _flyingList[i];
                if (npc.SensorTargets.WatchTargets.Count == 0) {
                    npc.TryWander();
                    continue;
                }
                var target = npc.SensorTargets.WatchTargets[0].Target;
                if (!npc.Chasing) {
                    npc.Chase(target);
                }
                if (npc.Projectile.ShootTimer.IsActive) {
                    continue;
                }
                if (Physics.Raycast(npc.Projectile.ShootPivot.position, npc.Projectile.ShootPivot.forward, out var hit, 50, LayerMasks.DefaultCollision)) {
                    if (UnityToEntityBridge.GetEntity(hit.collider) == target) {
                        npc.Stop();
                        npc.Projectile.Fire();
                    }
                }
            }
        }
    }
}
