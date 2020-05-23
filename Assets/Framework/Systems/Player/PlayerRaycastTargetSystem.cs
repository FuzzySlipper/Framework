using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerRaycastTargetSystem : SystemBase, IMainSystemUpdate {
        private TemplateList<PlayerRaycastTargetTemplate> _list;
        private ManagedArray<PlayerRaycastTargetTemplate>.RefDelegate _del;

        public PlayerRaycastTargetSystem() {
            TemplateFilter<PlayerRaycastTargetTemplate>.Setup();
            _list = EntityController.GetTemplateList<PlayerRaycastTargetTemplate>();
            _del = Update;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _list.Run(_del);
        }

        private void Update(ref PlayerRaycastTargetTemplate template) {
            if (template.Graph.CurrentTag == GraphNodeTags.Action && template.CurrentAction.Value != null) {
                template.Target.Set(PlayerInputSystem.GetMouseRaycastPosition(DistanceSystem.FromUnitGridDistance(template.CurrentAction.Value.Config.Range)));
            }
        }
    }

    public class PlayerRaycastTargetTemplate : BaseTemplate {

        private CachedComponent<AnimationGraphComponent> _graph = new CachedComponent<AnimationGraphComponent>();
        private CachedComponent<CommandTarget> _target = new CachedComponent<CommandTarget>();
        private CachedComponent<CurrentAction> _current = new CachedComponent<CurrentAction>();

        public RuntimeStateGraph Graph { get => _graph.Value.Value; }
        public CommandTarget Target { get => _target.Value; }
        public CurrentAction CurrentAction { get => _current.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _graph, _target, _current
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(AnimationGraphComponent),
                typeof(CommandTarget),
                typeof(PlayerRaycastTargeting),
            };
        }
    }
   
}
