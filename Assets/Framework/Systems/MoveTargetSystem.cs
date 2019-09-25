using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public class MoveTargetSystem : SystemBase, IReceive<SetMoveTarget>, IReceive<SetLookTarget> {
        public MoveTargetSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(MoveTarget)
            }));
        }

        public void Handle(SetMoveTarget arg) {
            var moveTarget = arg.Owner.Get<MoveTarget>();
            if (moveTarget == null) {
                return;
            }
            moveTarget.SetMoveTarget(arg);
        }

        public void Handle(SetLookTarget arg) {
            var moveTarget = arg.Owner.Get<MoveTarget>();
            if (moveTarget == null) {
                return;
            }
            if (arg.LookOnly) {
                moveTarget.SetLookOnlyTarget(arg.Target);
            }
            else {
                moveTarget.SetLookTarget(arg.Target);
            }
        }
    }
}
