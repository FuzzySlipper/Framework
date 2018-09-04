//using UnityEngine;
//using System.Collections;

//namespace PixelComrades {
//    public class AddModImpact : BaseImpact {

//        private IActorMod _mod;

//        public AddModImpact(IActorMod mod, float percentPower, ImpactTypes impactType, DamageTypes damageType, ActionFx actionFx) : base( percentPower, impactType, damageType, actionFx) {
//            _mod = mod;
//        }

//        public override void ApplyImpact(ImpactEvent impactEvent) {
//            _mod.Attach(impactEvent.Owner, impactEvent.TargetActor);
//        }
//    }
//}

