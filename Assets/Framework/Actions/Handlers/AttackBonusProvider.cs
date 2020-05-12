using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PixelComrades.DungeonCrawler;

namespace PixelComrades {
    
    
    [ActionProvider(Label)]
    public class AttackSavingThrowBonusTemp : IActionProvider, IRuleEventRun<CheckHitEvent> {

        private const string Label = "AttackSavingThrowBonusTemp";
        private const string Amount = Label + "Amount";
        private const string Uses = Label + "Uses";
        
        public void SetupEntity(Entity entity, SimpleDataLine lineData, DataEntry allData) {
            var component = entity.GetOrAdd<GenericDataComponent>();
            if (!int.TryParse(lineData.Config, out var uses)) {
                uses = 1;
            }
            component.SetData(Label, Label);
            component.SetData(Amount, lineData.Amount);
            component.SetData(Uses, uses);
        }

        public void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var component = ae.Action.Data;
            if (component != null) {
                ae.Target.RuleEvents.Handlers.Add(this);
                ae.Target.Data.SetData(Label, Label);
                ae.Target.Data.SetData(Amount, component.GetInt(Amount));
                ae.Target.Data.SetData(Uses, component.GetInt(Uses));
            }
        }

        public void RuleEventRun(ref CheckHitEvent context) {
            var targetData = context.Origin.Data;
            var targetChar = context.Origin;
            if (targetData != null && targetData.HasString(Label)) {
                context.AttackTotal += targetData.GetInt(Amount);
            }
            else {
                targetData = context.Target.Data;
                targetChar = context.Target;
                if (targetData != null && targetData.HasString(Label)) {
                    context.DefenseTotal += targetData.GetInt(Amount);
                }
                else {
                    targetData = null;
                }
            }
            if (targetData == null) {
                return;
            }
            var uses = targetData.GetInt(Uses);
            if (uses < 0) {
                return;
            }
            uses--;
            if (uses <= 0) {
                targetChar.RuleEvents.Handlers.Remove(this);
                targetData.RemoveString(Label);
                targetData.RemoveInt(Uses);
                targetData.RemoveInt(Amount);
            }
            else {
                targetData.SetData(Uses, uses);
            }
        }
    }
    [ActionProvider(Label)]
    public class AttackBonus : IActionProvider, IRuleEventRun<CheckHitEvent> {

        private const string Label = "AttackBonus";
        private const string Amount = Label + "Amount";
        private const string Uses = Label + "Uses";
        
        public void SetupEntity(Entity entity, SimpleDataLine lineData, DataEntry allData) {
            var component = entity.GetOrAdd<GenericDataComponent>();
            if (!int.TryParse(lineData.Config, out var uses)) {
                uses = 1;
            }
            component.SetData(Label, Label);
            component.SetData(Amount, lineData.Amount);
            component.SetData(Uses, uses);
        }

        public void OnUsage(ActionEvent ae, ActionCommand cmd) {
            var component = ae.Action.Data;
            if (component != null) {
                ae.Target.RuleEvents.Handlers.Add(this);
                ae.Target.Data.SetData(Label, Label);
                ae.Target.Data.SetData(Amount, component.GetInt(Amount));
                ae.Target.Data.SetData(Uses, component.GetInt(Uses));
            }
        }

        public void RuleEventRun(ref CheckHitEvent context) {
            var targetData = context.Origin.Data;
            var targetChar = context.Origin;
            if (targetData != null && targetData.HasString(Label)) {
                context.AttackTotal += targetData.GetInt(Amount);
            }
            else {
                return;
            }
            var uses = targetData.GetInt(Uses);
            if (uses < 0) {
                return;
            }
            uses--;
            if (uses <= 0) {
                targetChar.RuleEvents.Handlers.Remove(this);
                targetData.RemoveString(Label);
                targetData.RemoveInt(Uses);
                targetData.RemoveInt(Amount);
            }
            else {
                targetData.SetData(Uses, uses);
            }
        }
    }
}
