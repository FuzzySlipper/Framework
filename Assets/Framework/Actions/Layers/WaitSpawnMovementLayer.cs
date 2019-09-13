using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class WaitForSpawnMovement : ActionLayer, ISerializable {

        public string Data;

        private Entity _currentSpawn;

        public WaitForSpawnMovement(Action action, string data) : base(action) {
            Data = data;
        }

        public WaitForSpawnMovement(SerializationInfo info, StreamingContext context) : base(info, context) {
            Data = info.GetValue(nameof(Data), Data);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Data), Data);
        }
        
        public override void Start(ActionUsingNode node) {
            base.Start(node);
            var entity = node.Entity;
            var spawnEntity = World.Get<ProjectileSystem>().SpawnProjectile(entity, Data, node.ActionEvent, null);
            if (spawnEntity != null) {
                Vector3 target = node.ActionEvent.Target;
                spawnEntity.ParentId = entity.Id;
                spawnEntity.Add(new SimplerMover(spawnEntity));
                //spawnEntity.AddObserver(this);
                spawnEntity.Post(new StartMoveEvent(spawnEntity, target, null));
            }
        }

        public override void Evaluate(ActionUsingNode node) {
            if (!_currentSpawn.Tags.Contain(EntityTags.Moving)) {
                node.AdvanceEvent();
                _currentSpawn.Destroy(); // this doesn't respect pooling
                _currentSpawn = null;
            }
        }
    }
}
