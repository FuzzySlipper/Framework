using UnityEngine;

namespace PixelComrades {

    public class ImpactRendererComponent : ComponentBase {
        private IImpactRenderer _renderer;

        public ImpactRendererComponent(IImpactRenderer renderer) {
            _renderer = renderer;
        }

        public void PlayAnimation(SpriteAnimation animation, Color color) {
            _renderer.PlayAnimation(animation, color);
        }
    }

    public interface IImpactRenderer {
        void PlayAnimation(SpriteAnimation animation, Color color);
    }

    [CreateAssetMenu(menuName =  "Assets/ActionFx")]
    public class ActionFx : ScriptableObject {

        [SerializeField] private ActionFxData[] _actionData = new ActionFxData[0];

        public bool TryGetColor(out Color color) {
            for (int i = 0; i < _actionData.Length; i++) {
                switch (_actionData[i].Event) {
                    case ActionStateEvents.Collision:
                    case ActionStateEvents.CollisionOrImpact:
                    case ActionStateEvents.Impact:
                        if (_actionData[i].Particle.Animation != null) {
                            color = _actionData[i].Particle.Color;
                            return true;
                        }
                        break;
                }
            }
            for (int i = 0; i < _actionData.Length; i++) {
                if (_actionData[i].Particle.Animation != null) {
                    color = _actionData[i].Particle.Color;
                    return true;
                }
            }
            color = Color.red;
            return false;
        }

        public void TriggerEvent(ActionStateEvent actionEvent) {
            for (int i = 0; i < _actionData.Length; i++) {
                if (_actionData[i].Event == actionEvent.State || (_actionData[i].Event == ActionStateEvents.CollisionOrImpact && (actionEvent.State == ActionStateEvents.Impact || actionEvent.State == ActionStateEvents.Collision))) {
                    if (_actionData[i].Sound != null) {
                        AudioPool.PlayClip(_actionData[i].Sound, actionEvent.Position, 0.5f);
                    }
                    if (_actionData[i].Particle.Animation != null) {
                        switch (_actionData[i].Event) {
                            case ActionStateEvents.Collision:
                            case ActionStateEvents.CollisionOrImpact:
                            case ActionStateEvents.Impact:
                                if (actionEvent.Focus != null) {
                                    var impactRenderer = actionEvent.Focus.Get<ImpactRendererComponent>();
                                    if (impactRenderer != null) {
                                        impactRenderer.PlayAnimation(_actionData[i].Particle.Animation, _actionData[i].Particle.Color);
                                        continue;
                                    }
                                }
                                break;
                        }
                        //var spawn = ItemPool.SpawnScenePrefab(_actionPrefabs[i].Prefab, actionEvent.Position, actionEvent.Rotation);
                        //CheckObjectForListener(spawn, actionEvent);
                        var player = SpriteParticleSystem.PlayParticle(_actionData[i].Particle, actionEvent.Position, actionEvent.Rotation);
                        if (_actionData[i].Parent && actionEvent.Focus != null) {
                            var tr = actionEvent.Focus.FindTr();
                            if (tr != null) {
                                player.Tr.SetParent(tr);
                            }
                        }
                    }
                }
            }
        }

        private void CheckObjectForListener(GameObject newObject, ActionStateEvent stateEvent) {
            var actionListener = newObject.GetComponent<IActionPrefab>();
            if (actionListener != null) {
                actionListener.OnActionSpawn(stateEvent);
            }
        }
    }
}