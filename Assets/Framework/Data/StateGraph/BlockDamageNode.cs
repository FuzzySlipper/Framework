using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class BlockDamageNode : StateGraphNode {

        public float WaitTime;
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "Block Damage Node"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode, IRuleEventStart<PrepareDamageEvent> {
            
            private BlockDamageNode _originalNode;
            private BlockDamageAction _config;
            private ActionUsingTemplate _owner;
            private VitalStat _vitalStat;
            private bool _isWaiting;
            private ActionFx _fxComponent;
            private float _finalCost;
            private Entity _actionEntity;
            
            public RuntimeNode(BlockDamageNode node, RuntimeStateGraph graph) : base(node,graph) {
                _originalNode = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                var action = Graph.Entity.Get<CurrentAction>()?.Value;
                if (action == null) {
                    return;
                }
                _actionEntity = action.Entity;
                _owner = Graph.Entity.GetTemplate<ActionUsingTemplate>();
                _config = _actionEntity.Get<BlockDamageAction>();
                var model = ItemPool.Spawn(_config.ModelData);
                if (model != null) {
                    var spawnPivot = _actionEntity.Get<SpawnPivotComponent>();
                    spawnPivot.SetNewChild(model.Transform);
                    _actionEntity.Add(new RenderingComponent(model.GetComponent<IRenderingComponent>()));
                    _actionEntity.Add(new TransformComponent(model.transform));
                }
                if (!Graph.Entity.Tags.Contain(EntityTags.Player)) {
                    Graph.Entity.Add(new BlockDamageFlat());
                    _isWaiting = true;
                    _vitalStat = null;
                    return;
                }
                _vitalStat = _owner.Stats.GetVital(_config.TargetVital);
                var skillMulti = 1f;
                if (!string.IsNullOrEmpty(_config.Skill)) {
                    var skillValue = _owner.Entity.FindStatValue(_config.Skill);
                    skillMulti = Mathf.Clamp(1 - (skillValue * CostVital.SkillPercent), CostVital.SkillMaxReduction, 1);
                }
                Graph.Entity.GetOrAdd<RuleEventListenerComponent>().Handlers.Add(this);
                _fxComponent = action.Fx?.Value;
                _finalCost = _config.Cost * skillMulti;
            }

            public override void OnExit() {
                base.OnExit();
                _vitalStat = null;
                var tr = _actionEntity.Get<TransformComponent>();
                if (tr != null) {
                    ItemPool.Despawn(tr.gameObject);
                    _actionEntity.Remove(tr);
                    _actionEntity.Remove<RenderingComponent>();
                }
                if (_isWaiting) {
                    Graph.Entity.Remove<BlockDamageFlat>();
                }
                else {
                    Graph.Entity.Get<RuleEventListenerComponent>().Handlers.Remove(this);
                }
            }

            public override bool TryComplete(float dt) {
                if (base.TryComplete(dt)) {
                    return true;
                }
                if (_owner == null) {
                    return true;
                }
                if (_isWaiting) {
                    if (TimeManager.Time >= TimeEntered + _originalNode.WaitTime) {
                        return true;
                    }
                    return false;
                }
                if (!PlayerInputSystem.GetButton(_config.ChargeInput) || _vitalStat.Current < _config.Cost) {
                    return true;
                }
                return false;
            }

            public bool CanRuleEventStart(ref PrepareDamageEvent arg) {
                if (_vitalStat == null) {
                    return true;
                }
                if (_fxComponent != null) {
                    _fxComponent.TriggerEvent(
                        new ActionEvent(arg.Origin, arg.Target, arg.Impact.HitPoint, Quaternion.LookRotation(arg.Impact.HitNormal),
                            ActionState.Collision));
                }
                _vitalStat.Current -= _finalCost;
                World.Get<GameLogSystem>().StartNewMessage(out var log, out var hover);
                log.Append(arg.Target.GetName());
                log.Append(" completely blocked damage from ");
                log.Append(arg.Origin.GetName());
                World.Get<GameLogSystem>().PostCurrentStrings(GameLogSystem.DamageColor);
                return false;
            }
        }
    }
}
