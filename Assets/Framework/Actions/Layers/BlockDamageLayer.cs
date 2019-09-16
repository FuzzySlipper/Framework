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
            if (!node.Entity.Tags.Contain(EntityTags.Player)) {
                var dmgComponent = node.Entity.GetOrAdd<BlockDamage>();
                dmgComponent.Dels.Add(BlockDamage);
                _isWaiting = true;
                _vitalStat = null;
                return;
            }
            var blockDamageComponent = node.Entity.GetOrAdd<BlockDamageWithCostComponent>();
            _vitalStat = node.Stats.GetVital(TargetVital);
            var skillMulti = 1f;
            if (!string.IsNullOrEmpty(_skill)) {
                var skillValue = node.Entity.FindStatValue(_skill);
                skillMulti = Mathf.Clamp(1 - (skillValue * CostVital.SkillPercent), CostVital.SkillMaxReduction, 1);
            }
            blockDamageComponent.Assign(node.Entity, _vitalStat, Cost * skillMulti, node.ActionEvent.Action.Fx);
        }

        public override void End(ActionUsingNode node) {
            base.End(node);
            _vitalStat = null;
            var model = node.ActionEvent.Action.Entity.Get<ModelComponent>();
            if (model != null) {
                ItemPool.Despawn(model.Model.Tr.gameObject);
                node.ActionEvent.Action.Entity.Remove(model);
            }
            if (_isWaiting) {
                node.Entity.Remove<BlockDamage>();
            }
            else {
                node.Entity.Remove<BlockDamageWithCostComponent>();
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

        private bool BlockDamage(DamageEvent dmgEvent) {
            return true;
        }
    }

    [System.Serializable]
	public sealed class BlockDamageWithCostComponent : IComponent, IReceiveRef<DamageEvent>, IReceive<CollisionEvent> {

        private CachedStat<VitalStat> _stat;
        private float _cost;
        private ActionFx _fxComponent;

        public void Assign(Entity statOwner, VitalStat stat, float cost, ActionFx afx) {
            _stat = new CachedStat<VitalStat>(statOwner, stat);
            _cost = cost;
            _fxComponent = afx;
        }

        public void Clear() {
            _stat = null;
            _fxComponent = null;
        }

        public void Handle(CollisionEvent arg) {
            if (_stat == null || _fxComponent == null) {
                return;
            }
            _fxComponent.TriggerEvent(new ActionStateEvent(arg.Origin, arg.Target, arg.HitPoint + (arg.HitNormal * 0.1f), Quaternion.LookRotation(arg.HitNormal), ActionStateEvents.Collision));
        }

        public void Handle(ref DamageEvent arg) {
            if (_stat == null || arg.Amount <= 0) {
                return;
            }
            arg.Amount = 0;
            _stat.Stat.Current -= _cost;
        }
        
        public BlockDamageWithCostComponent(){}

        public BlockDamageWithCostComponent(SerializationInfo info, StreamingContext context) {
            _stat = info.GetValue(nameof(_stat), _stat);
            _cost = info.GetValue(nameof(_cost), _cost);
            _fxComponent = ItemPool.LoadAsset<ActionFx>(info.GetValue(nameof(_fxComponent), ""));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(_stat), _stat);
            info.AddValue(nameof(_cost), _cost);
            info.AddValue(nameof(_fxComponent), ItemPool.GetAssetLocation(_fxComponent));
        }
    }
}
