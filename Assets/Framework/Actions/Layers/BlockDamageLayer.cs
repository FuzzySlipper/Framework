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
                node.ParentSpawn(model.Transform);
                node.ActionEvent.Action.Entity.Add(new RenderingComponent(model.GetComponent<IRenderingComponent>()));
                node.ActionEvent.Action.Entity.Add(new TransformComponent(model.transform));
            }
            var dmgComponent = node.Entity.GetOrAdd<BlockDamage>();
            if (!node.Entity.Tags.Contain(EntityTags.Player)) {
                dmgComponent.DamageBlockers.Add(BlockDamageFlat);
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
            dmgComponent.CollisionHandlers.Add(EvadeDamageWithStats);
            _fxComponent = node.ActionEvent.Action.Fx;
            _finalCost = Cost * skillMulti;
        }

        public override void End(ActionUsingNode node) {
            base.End(node);
            _vitalStat = null;
            var tr = node.ActionEvent.Action.Entity.Get<TransformComponent>();
            if (tr != null) {
                ItemPool.Despawn(tr.gameObject);
                node.ActionEvent.Action.Entity.Remove(tr);
                node.ActionEvent.Action.Entity.Remove<RenderingComponent>();
            }
            var blockDamage = node.Entity.Get<BlockDamage>();
            if (blockDamage != null) {
                if (_isWaiting) {
                    blockDamage.DamageBlockers.Remove(BlockDamageFlat);
                }
                else {
                    blockDamage.CollisionHandlers.Remove(EvadeDamageWithStats);
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
            if (!PlayerInputSystem.GetButton(ChargeInput) || _vitalStat.Current < Cost) {
                node.AdvanceEvent();
            }
        }

        private bool BlockDamageFlat(TakeDamageEvent dmgEvent) {
            return true;
        }

        private int EvadeDamageWithStats(CollisionEvent arg) {
            if (_vitalStat == null) {
                return arg.Hit;
            }
            if (_fxComponent != null) {
                _fxComponent.TriggerEvent(
                    new ActionStateEvent(arg.Origin, arg.Target, arg.HitPoint, Quaternion.LookRotation(arg.HitNormal),
                    ActionStateEvents.Collision));                
            }
            
            _vitalStat.Current -= _finalCost;
            return CollisionResult.Miss;
        }
    }
}
