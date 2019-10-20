using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister, Priority(Priority.Lowest)]
    public sealed class AnimationGraphSystem : SystemBase, IMainSystemUpdate, IReceive<DeathEvent>, IReceive<ReceivedDamageEvent> {

        private ComponentArray<AnimationGraphComponent> _graphList;
        private ComponentArray<AnimationGraphComponent>.RefDelegate _graphDel;
        
        public AnimationGraphSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new [] {
                typeof(AnimationGraphComponent)
            }));
            _graphList = EntityController.GetComponentArray<AnimationGraphComponent>();
            _graphDel = RunUpdate;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _graphList.Run(RunUpdate);
        }

        private void RunUpdate(ref AnimationGraphComponent graphComponent) {
            graphComponent.Value.SetVariable(GraphVariables.IsMoving, graphComponent.GetEntity().Tags.Contain(EntityTags.Moving));
        }

        public void Handle(DeathEvent arg) {
            arg.Target.AnimGraph.TriggerGlobal(GraphTriggers.Death);
        }

        public void Handle(ReceivedDamageEvent arg) {
            if (arg.Amount <= 0) {
                return;
            }
            arg.Target.AnimGraph.TriggerGlobal(GraphTriggers.GetHit);
        }
    }
}
