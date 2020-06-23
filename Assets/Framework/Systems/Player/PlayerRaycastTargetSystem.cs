using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class PlayerRaycastTargetSystem : SystemBase, IMainSystemUpdate {
        private TemplateList<PlayerRaycastTargetTemplate> _list;
        private ManagedArray<PlayerRaycastTargetTemplate>.RefDelegate _del;

        public PlayerRaycastTargetSystem() {
            _list = EntityController.GetTemplateList<PlayerRaycastTargetTemplate>();
            _del = Update;
        }

        public void OnSystemUpdate(float dt, float unscaledDt) {
            _list.Run(_del);
        }

        private void Update(ref PlayerRaycastTargetTemplate template) {
<<<<<<< HEAD
            if (template.Graph.CurrentTag == GraphNodeTags.Action) {
                template.Target.Set(PlayerInputSystem.GetMouseRaycastPosition(template.Raycast.Range));
=======
            if (template.Graph.CurrentTag == GraphNodeTags.Action && template.CurrentAction.Value != null) {
                template.Target.Set(PlayerInputSystem.GetMouseRaycastPosition(DistanceSystem.FromUnitGridDistance(template.CurrentAction.Value.Config.Range)));
>>>>>>> FirstPersonAction
            }
        }
    }

    public class PlayerRaycastTargetTemplate : BaseTemplate {

        private CachedComponent<AnimationGraphComponent> _graph = new CachedComponent<AnimationGraphComponent>();
        private CachedComponent<CommandTarget> _target = new CachedComponent<CommandTarget>();
        private CachedComponent<PlayerRaycastTargeting> _raycast = new CachedComponent<PlayerRaycastTargeting>();

        public RuntimeStateGraph Graph { get => _graph.Value.Value; }
        public CommandTarget Target { get => _target.Value; }
        public PlayerRaycastTargeting Raycast { get => _raycast.Value; }
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _graph, _target, _raycast
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
