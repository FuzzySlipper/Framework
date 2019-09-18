using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [Serializable]
    public class BlockDamageLayer : ActionLayer, ISerializable {
        public float Cost { get; }
        public string TargetVital { get; }
        public string ChargeInput { get; }
        public float WaitTime { get; }
        public string ModelData { get; }

        private VitalStat _vitalStat;
        private string _skill;
        private bool _isWaiting;
        private ActionFx _fxComponent;
        private float _finalCost;
        
        public BlockDamageLayer(Action action, string modelData, string targetVital, float cost, string skill, string chargeInput, float waitTime) : base(action) {
            ModelData = modelData;
            Cost = cost;
            TargetVital = targetVital;
            ChargeInput = chargeInput;
            WaitTime = waitTime;
            _skill = skill;
        }

        public BlockDamageLayer(SerializationInfo info, StreamingContext context) : base(info, context) {
            Cost = info.GetValue(nameof(Cost), Cost);
            TargetVital = info.GetValue(nameof(TargetVital), TargetVital);
            ChargeInput = info.GetValue(nameof(ChargeInput), ChargeInput);
            WaitTime = info.GetValue(nameof(WaitTime), WaitTime);
            ModelData = info.GetValue(nameof(ModelData), ModelData);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Cost), Cost);
            info.AddValue(nameof(TargetVital), TargetVital);
            info.AddValue(nameof(ChargeInput), ChargeInput);
            info.AddValue(nameof(WaitTime), WaitTime);
            info.AddValue(nameof(ModelData), ModelData);
        }

        public override void Start(ActionUsingNode node) {
            base.Start(node);
            var model = ItemPool.Spawn(UnityDirs.Models, ModelData, Vector3.zero, Quaternion.identity);
            if (model != null) {
                model.transform.SetParentResetPos(node.ActionEvent.SpawnPivot != null ? node.ActionEvent.SpawnPivot : node.Tr);
                node.ActionEvent.Action.Entity.Add(new ModelComponent(model.GetComponent<IModelComponent>()));
            }
            var dmgComponent = node.Entity.GetOrAdd<BlockDamage>();
            if (!node.Entity.Tags.Contain(EntityTags.Player)) {
                dmgComponent.Dels.Add(BlockDamageFlat);
                _isWaiting = true;
                _vitalStat = null;
                return;
            }
            _vitalStat = node.Stats.GetVital(TargetVital);
            var skillMulti = 1f;
            if (!string.IsNullOrEmpty(_skill)) {
                var skillValue = node.Entity.FindStatValue(_skill);
                skillMulti = Mathf.Clamp(1 - (skillValue * CostVital.SkillPercent), CostVital.SkillMaxReduction, 1);
            }
            dmgComponent.Dels.Add(BlockDamageWithStats);
            _fxComponent = node.ActionEvent.Action.Fx;
            _finalCost = Cost * skillMulti;
        }

        public override void End(ActionUsingNode node) {
            base.End(node);
            _vitalStat = null;
            var model = node.ActionEvent.Action.Entity.Get<ModelComponent>();
            if (model != null) {
                ItemPool.Despawn(model.Model.Tr.gameObject);
                node.ActionEvent.Action.Entity.Remove(model);
            }
            var blockDamage = node.Entity.Get<BlockDamage>();
            if (blockDamage != null) {
                if (_isWaiting) {
                    blockDamage.Dels.Remove(BlockDamageFlat);
                }
                else {
                    blockDamage.Dels.Remove(BlockDamageWithStats);
                }
            }
        }

        public override void Evaluate(ActionUsingNode node) {
            if (_isWaiting) {
                if (TimeManager.Time >= node.ActionEvent.TimeStart + WaitTime) {
                    node.AdvanceEvent();
                }
                return;
            }
            if (!PlayerInput.main.GetKey(ChargeInput) || _vitalStat.Current < Cost) {
                node.AdvanceEvent();
            }
        }

        private bool BlockDamageFlat(DamageEvent dmgEvent) {
            return true;
        }

        public bool BlockDamageWithStats(DamageEvent arg) {
            if (_fxComponent != null) {
                CollisionExtensions.GenerateHitLocDir(arg.Origin.Tr, arg.Target.Tr, arg.Target.Collider, 
                    out var hitPoint, out var hitNormal);
                _fxComponent.TriggerEvent(
                    new ActionStateEvent(
                        arg.Origin, arg.Target, hitPoint + (hitNormal * 0.1f), Quaternion.LookRotation(hitNormal),
                        ActionStateEvents.Collision));                
            }
            if (_vitalStat == null || arg.Amount <= 0) {
                return false;
            }
            arg.Amount = 0;
            _vitalStat.Current -= _finalCost;
            return true;
        }
    }
}
