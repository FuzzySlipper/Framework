using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    public class AttackSavingThrowBonusTemp : ActionHandler, IRuleEventRun<CheckHitEvent> {

        private const string Label = "AttackSavingThrowBonusTemp";
        private const string AmountLabel = Label + "Amount";
        private const string UsesLabel = Label + "Uses";

        public int Amount;
        public int Uses;
        
        public override void SetupEntity(Entity entity) {
            var component = entity.GetOrAdd<GenericDataComponent>();
            component.SetData(Label, Label);
            component.SetData(AmountLabel, Amount);
            component.SetData(UsesLabel, Uses);
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var component = ae.Action.Data;
            if (component != null) {
                ae.Target.RuleEvents.Handlers.Add(this);
                ae.Target.GenericData.SetData(Label, Label);
                ae.Target.GenericData.SetData(AmountLabel, component.GetInt(AmountLabel));
                ae.Target.GenericData.SetData(UsesLabel, component.GetInt(UsesLabel));
            }
        }

        public void RuleEventRun(ref CheckHitEvent context) {
            var targetData = context.Origin.GenericData;
            var targetChar = context.Origin;
            if (targetData != null && targetData.HasString(Label)) {
                context.AttackTotal += targetData.GetInt(AmountLabel);
            }
            else {
                targetData = context.Target.GenericData;
                targetChar = context.Target;
                if (targetData != null && targetData.HasString(Label)) {
                    context.DefenseTotal += targetData.GetInt(AmountLabel);
                }
                else {
                    targetData = null;
                }
            }
            if (targetData == null) {
                return;
            }
            var uses = targetData.GetInt(UsesLabel);
            if (uses < 0) {
                return;
            }
            uses--;
            if (uses <= 0) {
                targetChar.RuleEvents.Handlers.Remove(this);
                targetData.RemoveString(Label);
                targetData.RemoveInt(UsesLabel);
                targetData.RemoveInt(AmountLabel);
            }
            else {
                targetData.SetData(UsesLabel, uses);
            }
        }
    }
    
    public class AttackBonus : ActionHandler, IRuleEventRun<CheckHitEvent> {

        private const string Label = "AttackBonus";
        private const string AmountLabel = Label + "Amount";
        private const string UsesLabel = Label + "Uses";
        
        public int Amount;
        public int Uses;
        
        public override void SetupEntity(Entity entity) {
            var component = entity.GetOrAdd<GenericDataComponent>();
            component.SetData(Label, Label);
            component.SetData(AmountLabel, Amount);
            component.SetData(UsesLabel, Uses);
        }

        public override void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var component = ae.Action.Data;
            if (component != null) {
                ae.Target.RuleEvents.Handlers.Add(this);
                ae.Target.GenericData.SetData(Label, Label);
                ae.Target.GenericData.SetData(AmountLabel, component.GetInt(AmountLabel));
                ae.Target.GenericData.SetData(UsesLabel, component.GetInt(UsesLabel));
            }
        }

        public void RuleEventRun(ref CheckHitEvent context) {
            var targetData = context.Origin.GenericData;
            var targetChar = context.Origin;
            if (targetData != null && targetData.HasString(Label)) {
                context.AttackTotal += targetData.GetInt(AmountLabel);
            }
            else {
                return;
            }
            var uses = targetData.GetInt(UsesLabel);
            if (uses < 0) {
                return;
            }
            uses--;
            if (uses <= 0) {
                targetChar.RuleEvents.Handlers.Remove(this);
                targetData.RemoveString(Label);
                targetData.RemoveInt(UsesLabel);
                targetData.RemoveInt(AmountLabel);
            }
            else {
                targetData.SetData(UsesLabel, uses);
            }
        }
    }
}
