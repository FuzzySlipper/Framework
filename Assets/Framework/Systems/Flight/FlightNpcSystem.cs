﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlightNpcSystem : SystemBase, IMainSystemUpdate {

        private NodeList<NpcFlyingNode> _flyingList;
        private ManagedArray<NpcFlyingNode>.RefDelegate _del;

        public FlightNpcSystem() {
            NodeFilter<NpcFlyingNode>.Setup(NpcFlyingNode.GetTypes());
            _flyingList = EntityController.GetNodeList<NpcFlyingNode>();
            _del = UpdateNode;
        }

        public override void Dispose() {
            base.Dispose();
            if (_flyingList != null) {
                _flyingList.Clear();
            }
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            if (Game.Paused || !Game.GameActive) {
                return;
            }
            _flyingList.Run(_del);
        }

        private void UpdateNode(ref NpcFlyingNode npc) {
            if (npc.SensorTargets.WatchTargets.Count == 0) {
                npc.TryWander();
                return;
            }
            var target = npc.SensorTargets.WatchTargets[0].Target;
            if (!npc.Chasing) {
                npc.Chase(target);
            }
            if (npc.Projectile.ShootTimer.IsActive) {
                return;
            }
            if (Physics.Raycast(
                npc.Projectile.ShootPivot.Tr.position, npc.Projectile.ShootPivot.Tr.forward, out var hit, 50,
                LayerMasks.DefaultCollision)) {
                if (UnityToEntityBridge.GetEntity(hit.collider) == target) {
                    npc.Stop();
                    npc.Projectile.Fire();
                }
            }
        }
    }
}
